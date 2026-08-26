using DigitalArs.Domain.Entities;

using Microsoft.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalArs.Infrastructure.Data.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        // Le indicamos a EF Core que las transacciones se van a guardar
        // en una tabla llamada Transactions.
        builder.ToTable("Transactions");

        // Id será la clave primaria de la tabla.
        builder.HasKey(t => t.Id);

        // El Id se genera automáticamente cuando se crea una nueva transacción.
        builder.Property(t => t.Id)
            .ValueGeneratedOnAdd();

        // Amount representa el importe de la transacción.
        //
        // Usamos decimal(18,2) porque estamos trabajando con dinero.
        // Los 2 decimales permiten guardar valores como 100.50.
        builder.Property(t => t.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        // Guardamos la fecha y hora en la que se realizó la transacción.
        //
        // datetime2 es el tipo recomendado en SQL Server cuando necesitamos
        // almacenar fechas y horas con buena precisión.
        builder.Property(t => t.Date)
            .IsRequired()
            .HasColumnType("datetime2");

        // TransactionType es un enum en nuestro código.
        //
        // Por defecto EF Core podría guardarlo como un número (0, 1, 2...).
        // En este caso preferimos guardarlo como texto para que sea más fácil
        // entender el valor directamente desde la base de datos.
        //
        // Por ejemplo, en lugar de guardar "0", podemos tener "Deposit".
        builder.Property(t => t.TransactionType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // Creamos un índice sobre Date porque es muy probable que las consultas
        // del historial necesiten ordenar o filtrar las transacciones por fecha.
        //
        // Esto ayuda a la base de datos a encontrar esos registros más rápido.
        builder.HasIndex(t => t.Date)
            .HasDatabaseName("IX_Transactions_Date");

        // Esta relación representa la cuenta desde la que sale el dinero.
        //
        // FromAccountId en Transaction es la clave foránea que apunta
        // a la cuenta de origen.
        //
        // Usamos Restrict para evitar que una cuenta pueda eliminarse
        // si todavía tiene transacciones asociadas.
        builder.HasOne(t => t.FromAccount)
            .WithMany(a => a.TransactionsFrom)
            .HasForeignKey(t => t.FromAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        // Esta relación representa la cuenta que recibe el dinero.
        //
        // ToAccountId es la clave foránea que apunta a la cuenta de destino.
        //
        // También usamos Restrict. Además de proteger el historial de
        // transacciones, evita problemas de borrado en cascada porque
        // Transaction tiene dos relaciones distintas con Account.
        builder.HasOne(t => t.ToAccount)
            .WithMany(a => a.TransactionsTo)
            .HasForeignKey(t => t.ToAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}