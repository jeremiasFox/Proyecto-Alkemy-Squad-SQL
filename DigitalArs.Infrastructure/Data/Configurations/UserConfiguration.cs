using DigitalArs.Domain.Entities;

using Microsoft.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalArs.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Le indicamos a EF Core que los usuarios se van a guardar
        // en una tabla llamada Users.
        builder.ToTable("Users");

        // Id será la clave primaria de la tabla.
        builder.HasKey(u => u.Id);

        // El Id se genera automáticamente cuando se crea un nuevo usuario.
        builder.Property(u => u.Id)
            .ValueGeneratedOnAdd();

        // El nombre del usuario es obligatorio y puede tener hasta
        // 100 caracteres.
        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(100);

        // El email es obligatorio y puede tener hasta 256 caracteres.
        //
        // La validación de formato del email debería hacerse en otra capa.
        // Acá nos encargamos de cómo se guarda y de sus restricciones
        // a nivel de base de datos.
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        // Además de mejorar las búsquedas por email, el índice único
        // garantiza que no puedan existir dos usuarios con el mismo email.
        //
        // Esto es especialmente importante para un login, donde el email
        // identifica de forma única al usuario.
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_Users_Email");

        // La contraseña es obligatoria y permite hasta 256 caracteres.
        //
        // Este campo no debería contener nunca la contraseña original.
        // La aplicación debe guardar únicamente el hash de la contraseña.
        builder.Property(u => u.Password)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Cada usuario tiene un único Role, mientras que un mismo Role
        // puede estar asociado a muchos usuarios.
        //
        // Por ejemplo, muchos usuarios pueden tener el rol "User",
        // mientras que todos ellos apuntan al mismo registro de Role.
        //
        // RoleId es la clave foránea que relaciona Users con Roles.
        builder.HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Cada usuario tiene una única Account y cada Account pertenece
        // a un único usuario.
        //
        // En este caso la clave foránea está en Account (UserId), por eso
        // usamos HasForeignKey<Account>().
        //
        // Restrict evita que se pueda eliminar un usuario que todavía
        // tenga una cuenta asociada.
        builder.HasOne(u => u.Account)
            .WithOne(a => a.User)
            .HasForeignKey<Account>(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Datos iniciales de usuarios de prueba.
        builder.HasData(
            new User { Id = 1, Name = "Admin",  Email = "admin@digitalars.com", Password = "$2a$11$xmyuGpto0QJ8OOurvxKshObliC8zGPQ2uYP1xXltbQv8k.Ansgg5S", RoleId = 1, IsActive = true },
            new User { Id = 2, Name = "User1",  Email = "user1@digitalars.com", Password = "$2a$11$p1UYTEKIRVPO3kh15hpFCOxK7DeNAd9uzdGKOujq9S33qs0qwXHce", RoleId = 2, IsActive = true },
            new User { Id = 3, Name = "User2",  Email = "user2@digitalars.com", Password = "$2a$11$QedlI7cVp7uZ0k.tyCCJ/.a/9TkcCX2FjvXJHU6ebVg5fvYlCWB0i", RoleId = 2, IsActive = true }
        );

    }
}