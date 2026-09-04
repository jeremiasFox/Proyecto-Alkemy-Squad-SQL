using DigitalArs.Application.Common.DTOs;
using DigitalArs.Application.DTOs.Transaction;

namespace DigitalArs.Application.Common.Interfaces;

public interface ITransactionService
{
    Task<TransferResponseDto> TransferAsync(
        int userId,
        TransferRequestDto request,
        CancellationToken cancellationToken = default);

    Task<PaginatedResponseDto<TransactionResponseDto>> GetMyTransactionsAsync(
        int userId,
        TransactionFilterDto filter,
        CancellationToken cancellationToken = default);
}
