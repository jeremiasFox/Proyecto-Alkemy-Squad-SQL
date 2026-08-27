using DigitalArs.Domain.Entities;

using DigitalArs.Domain.Interfaces;

using DigitalArs.Infrastructure.Data;

using Microsoft.EntityFrameworkCore.Storage;

namespace DigitalArs.Infrastructure.Repositories;

// El UnitOfWork se encarga de coordinar los distintos repositorios
// utilizando un mismo DbContext.
//
// Esto permite que varias operaciones realizadas sobre diferentes
// repositorios puedan guardarse juntas y formar parte de una misma
// transacción cuando sea necesario.
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    // Guarda la transacción que está activa actualmente.
    // Si es null, significa que estamos trabajando sin una transacción
    // explícita.
    private IDbContextTransaction? _currentTransaction;

    // Los repositorios se crean solamente cuando se necesitan.
    //
    // Por ejemplo, si una operación solo trabaja con Users, no es necesario
    // crear los repositorios de Account, Role y Transaction.
    //
    // Todos los repositorios utilizan el mismo DbContext.
    private IRepository<Account>? _accounts;
    private IRepository<Role>? _roles;
    private IRepository<User>? _users;
    private IRepository<Transaction>? _transactions;

    // Recibimos el DbContext mediante inyección de dependencias.
    //
    // Este mismo contexto será utilizado por todos los repositorios
    // creados por este UnitOfWork.
    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    // Cada propiedad crea su repositorio la primera vez que se utiliza.
    //
    // El operador ??= significa: si el repositorio todavía no existe,
    // créalo; si ya existe, reutiliza la misma instancia.
    public IRepository<Account> Accounts
        => _accounts ??= new GenericRepository<Account>(_context);

    public IRepository<Role> Roles
        => _roles ??= new GenericRepository<Role>(_context);

    public IRepository<User> Users
        => _users ??= new GenericRepository<User>(_context);

    public IRepository<Transaction> Transactions
        => _transactions ??= new GenericRepository<Transaction>(_context);

    // Guarda todos los cambios que están pendientes en el DbContext.
    //
    // Los repositorios se encargan de agregar, modificar o eliminar entidades,
    // pero es el UnitOfWork quien finalmente confirma esos cambios.
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    // Inicia una nueva transacción en la base de datos.
    //
    // Primero comprobamos que no haya otra transacción activa. La idea es
    // mantener una única transacción controlada por este UnitOfWork.
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
            throw new InvalidOperationException(
                "Ya existe una transacción activa. Confirme o revierta la transacción actual antes de iniciar una nueva.");

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    // Confirma la transacción actual.
    //
    // Primero guardamos los cambios pendientes y después hacemos el Commit.
    // Si alguna de estas operaciones falla, la transacción no se considera
    // confirmada.
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
            throw new InvalidOperationException(
                "No hay ninguna transacción activa. Llame a BeginTransactionAsync antes de confirmar.");

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _currentTransaction.CommitAsync(cancellationToken);
        }
        finally
        {
            // Liberamos la transacción independientemente de si el Commit
            // terminó correctamente o produjo una excepción.
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    // Revierte la transacción actual cuando algo salió mal.
    //
    // Al hacer Rollback, los cambios realizados dentro de la transacción
    // no quedan confirmados en la base de datos.
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
            throw new InvalidOperationException(
                "No hay ninguna transacción activa. Llame a BeginTransactionAsync antes de revertir.");

        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            // Una vez terminada la transacción, liberamos el recurso
            // y dejamos la referencia en null para poder iniciar otra
            // transacción más adelante.
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    // Liberamos los recursos utilizados por el UnitOfWork.
    //
    // El contenedor de inyección de dependencias se encarga de llamar
    // a Dispose al finalizar el scope del request.
    //
    // También nos aseguramos de liberar una posible transacción que
    // todavía esté abierta y, finalmente, el DbContext.
    public void Dispose()
    {
        _currentTransaction?.Dispose();
        _context.Dispose();
    }
}