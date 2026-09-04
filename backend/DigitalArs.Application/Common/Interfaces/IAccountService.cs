using DigitalArs.Application.DTOs.Account;

namespace DigitalArs.Application.Common.Interfaces;

public interface IAccountService
{
    Task<DepositResponseDto> DepositAsync(
        int userId,
        DepositRequestDto request,
        CancellationToken cancellationToken = default);
}