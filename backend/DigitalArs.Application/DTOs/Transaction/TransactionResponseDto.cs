using DigitalArs.Domain.Entities;

namespace DigitalArs.Application.DTOs.Transaction;

public class TransactionResponseDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Type { get; set; } = string.Empty;
    public int FromAccountId { get; set; }
    public int ToAccountId { get; set; }
}
