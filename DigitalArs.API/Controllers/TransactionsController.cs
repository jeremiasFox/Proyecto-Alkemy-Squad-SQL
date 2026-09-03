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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Transfer(
        [FromBody] TransferRequestDto request,
        CancellationToken cancellationToken)
    {
        // Verificamos que los datos recibidos cumplan con las validaciones
        // definidas en el DTO antes de procesar la transferencia.
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
              .SelectMany(v => v.Errors)
              .Select(e => e.ErrorMessage);
            throw new ValidationException(errors);
        }

        // Obtenemos el ID del usuario autenticado desde el token JWT.
        var userId = User.GetUserId();

        // Delegamos la lógica de negocio al servicio de transacciones
        // para realizar la transferencia entre cuentas.
        var result = await _transactionService.TransferAsync(userId, request, cancellationToken);
        return Ok(result);
    }
}