using DigitalArs.Application.Common.DTOs;
using DigitalArs.Application.Common.DTOs.Interfaces;
using DigitalArs.Application.Common.Interfaces;
using DigitalArs.Application.DTOs.Transaction;
using DigitalArs.Application.Exceptions;
using DigitalArs.Domain.Entities;
using DigitalArs.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DigitalArs.Application.Services;

public class TransactionService : ITransactionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppDbContext _context;

    public TransactionService(IUnitOfWork unitOfWork, IAppDbContext context)
    {
        _unitOfWork = unitOfWork;
        _context    = context;
    }

    public async Task<TransferResponseDto> TransferAsync(
        int userId,
        TransferRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // Obtener la cuenta origen del usuario autenticado
        var sourceAccount = (await _unitOfWork.Accounts
            .FindAsync(a => a.UserId == userId))
            .FirstOrDefault();

        if (sourceAccount is null)
            throw new NotFoundException("Cuenta", userId);

        // Validar que no sea una autotransferencia
        if (sourceAccount.Id == request.DestinationAccountId)
            throw new AppException("No se puede transferir dinero a la misma cuenta.");

        // Obtener la cuenta destino
        var destinationAccount = await _unitOfWork.Accounts
            .GetByIdAsync(request.DestinationAccountId);

        if (destinationAccount is null)
            throw new NotFoundException("Cuenta destino", request.DestinationAccountId);

        // Validar saldo suficiente
        if (sourceAccount.Balance < request.Amount)
            throw new AppException("Saldo insuficiente para realizar la transferencia.");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var date = DateTime.UtcNow;

            // Actualizar saldos
            sourceAccount.Balance      -= request.Amount;
            destinationAccount.Balance += request.Amount;

            _unitOfWork.Accounts.Update(sourceAccount);
            _unitOfWork.Accounts.Update(destinationAccount);

            // Registro de salida en la cuenta origen
            await _unitOfWork.Transactions.AddAsync(new Transaction
            {
                Amount          = request.Amount,
                Date            = date,
                FromAccountId   = sourceAccount.Id,
                ToAccountId     = destinationAccount.Id,
                TransactionType = TransactionType.TransferOut
            });

            // Registro de entrada en la cuenta destino
            await _unitOfWork.Transactions.AddAsync(new Transaction
            {
                Amount          = request.Amount,
                Date            = date,
                FromAccountId   = sourceAccount.Id,
                ToAccountId     = destinationAccount.Id,
                TransactionType = TransactionType.TransferIn
            });

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return new TransferResponseDto
            {
                FromAccountId = sourceAccount.Id,
                ToAccountId   = destinationAccount.Id,
                Amount        = request.Amount,
                NewBalance    = sourceAccount.Balance,
                Date          = date
            };
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<PaginatedResponseDto<TransactionResponseDto>> GetMyTransactionsAsync(
        int userId,
        TransactionFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        // Obtener el id de la cuenta del usuario autenticado
        var account = await _context.Accounts
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .Select(a => new { a.Id })
            .FirstOrDefaultAsync(cancellationToken);

        if (account is null)
            throw new NotFoundException("La cuenta no existe.");

        // Query base según el tipo de filtro:
        // - TransferIn:  solo los que llegaron a esta cuenta (ToAccountId)
        // - TransferOut: solo los que salieron de esta cuenta (FromAccountId)
        // - Deposit:     solo los depósitos de esta cuenta
        // - Sin filtro:  todos los movimientos relevantes sin duplicados
        IQueryable<Transaction> query;

        if (filter.Type == TransactionType.TransferIn)
        {
            // Lo que llegó: la cuenta es el destino
            query = _context.Transactions
                .AsNoTracking()
                .Where(t => t.ToAccountId == account.Id && t.TransactionType == TransactionType.TransferIn);
        }
        else if (filter.Type == TransactionType.TransferOut)
        {
            // Lo que salió: la cuenta es el origen
            query = _context.Transactions
                .AsNoTracking()
                .Where(t => t.FromAccountId == account.Id && t.TransactionType == TransactionType.TransferOut);
        }
        else if (filter.Type == TransactionType.Deposit)
        {
            query = _context.Transactions
                .AsNoTracking()
                .Where(t => t.ToAccountId == account.Id && t.TransactionType == TransactionType.Deposit);
        }
        else
        {
            // Sin filtro de tipo: mostrar cada movimiento una sola vez
            // desde la perspectiva del usuario.
            // TransferOut y Deposit: la cuenta es origen (FromAccountId)
            // TransferIn: la cuenta es destino (ToAccountId)
            query = _context.Transactions
                .AsNoTracking()
                .Where(t =>
                    (t.TransactionType == TransactionType.TransferOut && t.FromAccountId == account.Id) ||
                    (t.TransactionType == TransactionType.TransferIn  && t.ToAccountId   == account.Id) ||
                    (t.TransactionType == TransactionType.Deposit     && t.ToAccountId   == account.Id));
        }

        if (filter.DateFrom.HasValue)
            query = query.Where(t => t.Date >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            query = query.Where(t => t.Date <= filter.DateTo.Value);

        if (filter.AmountMin.HasValue)
            query = query.Where(t => t.Amount >= filter.AmountMin.Value);

        if (filter.AmountMax.HasValue)
            query = query.Where(t => t.Amount <= filter.AmountMax.Value);

        // Contar el total ANTES de paginar para los metadatos
        var totalCount = await query.CountAsync(cancellationToken);

        // Proyección directa a DTO: evita cargar las nav-properties (FromAccount,
        // ToAccount) y el problema N+1. Select genera un único SELECT en SQL.
        var items = await query
            .OrderByDescending(t => t.Date)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(t => new TransactionResponseDto
            {
                Id            = t.Id,
                Amount        = t.Amount,
                Date          = t.Date,
                Type          = t.TransactionType.ToString(),
                FromAccountId = t.FromAccountId,
                ToAccountId   = t.ToAccountId
            })
            .ToListAsync(cancellationToken);

        return new PaginatedResponseDto<TransactionResponseDto>
        {
            Items      = items,
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize   = filter.PageSize
        };
    }
}
