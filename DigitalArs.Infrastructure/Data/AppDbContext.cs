using DigitalArs.Domain.Entities;
using DigitalArs.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace DigitalArs.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Registra automáticamente todas las IEntityTypeConfiguration<T> que se encuentren en el ensamblado actual (Infrastructure).
        // Esto incluye: RoleConfiguration, UserConfiguration,AccountConfiguration y TransactionConfiguration.
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}