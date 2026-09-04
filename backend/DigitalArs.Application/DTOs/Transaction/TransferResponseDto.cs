namespace DigitalArs.Application.DTOs.Transaction;

public class TransferResponseDto
{
    public int FromAccountId { get; set; }
    public int ToAccountId { get; set; }
    public decimal Amount { get; set; }
    public decimal NewBalance { get; set; }
    public DateTime Date { get; set; }
}
