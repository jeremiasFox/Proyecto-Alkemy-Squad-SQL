namespace DigitalArs.Domain.Entities
{
    public class Account
    {
        public int Id { get; set; }
        public decimal Balance { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        // Transacciones realizadas desde esta cuenta.
        public ICollection<Transaction> TransactionsFrom { get; set; } = new List<Transaction>();

        // Transacciones recibidas en esta cuenta.
        public ICollection<Transaction> TransactionsTo { get; set; } = new List<Transaction>();
    }
}

// Almacena la cuenta bancaria del usuario