namespace DigitalArs.Application.DTOs.Account;

public class DepositResponseDto
{
    public int AccountId { get; set; }
    public decimal Amount { get; set; }
    public decimal Balance { get; set; }
    public DateTime Date { get; set; }
}