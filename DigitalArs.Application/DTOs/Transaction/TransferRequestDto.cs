using System.ComponentModel.DataAnnotations;

namespace DigitalArs.Application.DTOs.Transaction;

public class TransferRequestDto
{
    [Required]
    public int DestinationAccountId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0.")]
    public decimal Amount { get; set; }
}
