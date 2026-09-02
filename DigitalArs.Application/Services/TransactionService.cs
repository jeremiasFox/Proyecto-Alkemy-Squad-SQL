using DigitalArs.Application.Common.Interfaces;
using DigitalArs.Application.DTOs.Transaction;
using DigitalArs.Application.Exceptions;
using DigitalArs.Domain.Entities;
using DigitalArs.Domain.Interfaces;

namespace DigitalArs.Application.Services;

public class TransactionService : ITransactionService
{
    private readonly IUnitOfWork _unitOfWork;

    public TransactionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
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
}
