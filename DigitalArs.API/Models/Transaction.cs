namespace DigitalArs.API.Models
{
    // Representa cada movimiento de plata del banco
    public class Transaction
    {
        public int Id { get; set; }
        public decimal Amount { get; set; } // Monto exacto, por eso decimal
        public DateTime Date { get; set; } // Cuando se hizo
        public int FromAccountId { get; set; } // De que cuenta sale
        public int ToAccountId { get; set; } // A que cuenta entra
        public int TransactionTypeId { get; set; } // Que tipo es (deposito, retiro, etc)
        public TransactionType TransactionType { get; set; } = null!;
    }
}