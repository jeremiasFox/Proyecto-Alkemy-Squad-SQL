using DigitalArs.API.Helpers;
using DigitalArs.Application.Common.Interfaces;
using DigitalArs.Application.DTOs.Transaction;
using DigitalArs.Application.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalArs.API.Controllers;

[ApiController]
[Route("api/transactions")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer(
        [FromBody] TransferRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);
            throw new ValidationException(errors);
        }

        var userId = User.GetUserId();
        var result = await _transactionService.TransferAsync(userId, request, cancellationToken);
        return Ok(result);
    }
}
