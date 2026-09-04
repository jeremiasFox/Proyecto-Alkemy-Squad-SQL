namespace DigitalArs.Domain.Entities
{
    // Representa cada movimiento de plata del banco
    public class Transaction
    {
        public int Id { get; set; }
        public decimal Amount { get; set; } // Monto exacto, por eso decimal
        public DateTime Date { get; set; } // Cuando se hizo
        public int FromAccountId { get; set; } // De que cuenta sale
        public int ToAccountId { get; set; } // A que cuenta entra

        // Cuenta desde la que se realiza la transacción.
        public Account FromAccount { get; set; } = null!;

        // Cuenta a la que se envía el dinero.
        public Account ToAccount { get; set; } = null!;

        // Tipo de transacción: depósito, transferencia de entrada o transferencia de salida.
        public TransactionType TransactionType { get; set; } // Que tipo es (deposito, retiro, etc)
    }
}