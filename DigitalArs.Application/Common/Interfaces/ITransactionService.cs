using DigitalArs.Application.DTOs.Transaction;

namespace DigitalArs.Application.Common.Interfaces;

public interface ITransactionService
{
    Task<TransferResponseDto> TransferAsync(
        int userId,
        TransferRequestDto request,
        CancellationToken cancellationToken = default);
}
