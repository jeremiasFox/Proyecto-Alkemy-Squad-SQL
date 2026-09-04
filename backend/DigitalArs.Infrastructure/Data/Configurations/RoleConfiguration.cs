using DigitalArs.Domain.Entities;

using Microsoft.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalArs.Infrastructure.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        // Le decimos a EF Core que esta entidad se va a guardar
        // en una tabla llamada Roles.
        builder.ToTable("Roles");

        // Id será la clave primaria de la tabla.
        builder.HasKey(r => r.Id);

        // El Id se genera automáticamente cuando se crea un nuevo rol.
        builder.Property(r => r.Id)
            .ValueGeneratedOnAdd();

        // Name es obligatorio y no puede superar los 50 caracteres.
        //
        // Esto también hace que EF Core genere la columna con una
        // longitud máxima definida en la base de datos.
        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(50);

        // Un rol puede estar asignado a muchos usuarios.
        // Por ejemplo, el rol "Admin" puede pertenecer a varios Users.
        //
        // RoleId en User es la clave foránea que relaciona al usuario
        // con su rol.
        //
        // Usamos Restrict para evitar que un rol pueda eliminarse
        // mientras tenga usuarios asociados. De esta forma evitamos
        // borrar usuarios accidentalmente al eliminar un rol.
        builder.HasMany(r => r.Users)
            .WithOne(u => u.Role)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Datos iniciales de roles.
        builder.HasData(
            new Role
            {
                Id = 1,
                Name = "Admin"
            },
            new Role
            {
                Id = 2,
                Name = "User"
            }
        );

    }
}