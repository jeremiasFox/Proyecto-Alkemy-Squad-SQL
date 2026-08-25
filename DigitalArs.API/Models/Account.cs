namespace DigitalArs.API.Models
{
    public class Account
    {
        public int Id { get; set; }
        public decimal Balance { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}

// Almacena la cuenta bancaria del usuario