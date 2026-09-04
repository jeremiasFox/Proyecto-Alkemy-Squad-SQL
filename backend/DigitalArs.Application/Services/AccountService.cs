using DigitalArs.Application.Common.Interfaces;
using DigitalArs.Application.Common.Settings;
using DigitalArs.Application.DTOs.Account;
using DigitalArs.Domain.Entities;
using DigitalArs.Domain.Interfaces;
using Microsoft.Extensions.Options;
using DigitalArs.Application.Exceptions;

namespace DigitalArs.Application.Services;

public class AccountService : IAccountService
{
    private readonly IUnitOfWork _unitOfWork;

    private readonly DepositSettings _depositSettings;

    public AccountService(
    IUnitOfWork unitOfWork,
    IOptions<DepositSettings> depositSettings)
    {
        _unitOfWork = unitOfWork;
        _depositSettings = depositSettings.Value;
    }

    public async Task<DepositResponseDto> DepositAsync(
        int userId,
        DepositRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0)
            throw new ValidationException("El monto debe ser mayor a 0.");

        if (request.Amount > _depositSettings.MaxAmount)
            throw new ValidationException(
    $"El monto máximo permitido por depósito es {_depositSettings.MaxAmount}.");

        var account = (await _unitOfWork.Accounts
            .FindAsync(a => a.UserId == userId))
            .FirstOrDefault();

        if (account is null)
            throw new NotFoundException("La cuenta no existe.");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            account.Balance += request.Amount;

            _unitOfWork.Accounts.Update(account);

            var transaction = new Transaction
            {
                Amount = request.Amount,
                Date = DateTime.UtcNow,
                FromAccountId = account.Id,
                ToAccountId = account.Id,
                TransactionType = TransactionType.Deposit
            };

            await _unitOfWork.Transactions.AddAsync(transaction);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return new DepositResponseDto
            {
                AccountId = account.Id,
                Amount = request.Amount,
                Balance = account.Balance,
                Date = transaction.Date
            };
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}