using DigitalArs.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalArs.Infrastructure.Data.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        // Indicamos el nombre de la tabla que va a representar a Account dentro de la base de datos.
        builder.ToTable("Accounts");

        // Definimos Id como la clave primaria de la tabla.
        builder.HasKey(a => a.Id);

        // El Id se genera automáticamente cuando se crea una nueva cuenta.
        // Normalmente esto se traduce en un campo autoincremental en la BD.
        builder.Property(a => a.Id)
            .ValueGeneratedOnAdd();

        // Balance es obligatorio y se guarda como decimal(18,2), que es
        // un formato habitual para manejar valores monetarios. Si no se especifica un valor al crear la cuenta, comienza en 0.
        builder.Property(a => a.Balance)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0m);

        // Creamos un índice sobre UserId porque vamos a consultar frecuentemente la cuenta asociada a un usuario.     
        builder.HasIndex(a => a.UserId)
            .HasDatabaseName("IX_Accounts_UserId");

        // Una cuenta pertenece a un único usuario y un usuario tiene una única cuenta.
        // UserId es la clave foránea que relaciona ambas entidades.
        builder.HasOne(a => a.User)
            .WithOne(u => u.Account)
            .HasForeignKey<Account>(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Una cuenta puede tener muchas transacciones en las que
        // aparece como la cuenta de origen.
        //
        // FromAccountId en Transaction es la FK que apunta a Account.
        //
        // Usamos Restrict para evitar que al eliminar una cuenta
        // se eliminen automáticamente sus transacciones.
        builder.HasMany(a => a.TransactionsFrom)
            .WithOne(t => t.FromAccount)
            .HasForeignKey(t => t.FromAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        // Una cuenta también puede tener muchas transacciones en las
        // que aparece como la cuenta de destino.
        //
        // ToAccountId en Transaction es la FK que apunta a Account.
        //
        // Al igual que en la relación anterior, usamos Restrict para
        // evitar borrados en cascada y posibles conflictos en SQL Server,
        // ya que Transaction tiene dos relaciones con Account.
        builder.HasMany(a => a.TransactionsTo)
            .WithOne(t => t.ToAccount)
            .HasForeignKey(t => t.ToAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        // Datos iniciales de cuentas asociadas a los usuarios de prueba.
        builder.HasData(
            new Account
            {
                Id = 1,
                Balance = 10000m,
                UserId = 1
            },
            new Account
            {
                Id = 2,
                Balance = 5000m,
                UserId = 2
            },
            new Account
            {
                Id = 3,
                Balance = 7500m,
                UserId = 3
            }
        );

    }
}