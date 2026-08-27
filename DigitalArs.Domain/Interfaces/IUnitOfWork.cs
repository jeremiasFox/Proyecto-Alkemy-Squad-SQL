using DigitalArs.Domain.Entities;

namespace DigitalArs.Domain.Interfaces;

// El Unit of Work se encarga de coordinar los distintos repositorios
// y de controlar cuándo se guardan los cambios en la base de datos.
//
// La idea es que una operación que involucra varias entidades pueda
// confirmarse como un único bloque. Si algo falla, podemos revertir
// los cambios y evitar que la base de datos quede en un estado incompleto.
//
// La capa de aplicación trabaja con esta interfaz en lugar de acceder
// directamente a AppDbContext. De esta forma, mantenemos separada la
// lógica de negocio de los detalles de Entity Framework.
public interface IUnitOfWork : IDisposable
{
    // Repositorio utilizado para trabajar con las cuentas.
    IRepository<Account> Accounts { get; }

    // Repositorio utilizado para trabajar con los roles.
    IRepository<Role> Roles { get; }

    // Repositorio utilizado para trabajar con los usuarios.
    IRepository<User> Users { get; }

    // Repositorio utilizado para consultar y modificar transacciones.
    IRepository<Transaction> Transactions { get; }

    // Guarda en la base de datos todos los cambios pendientes.
    //
    // Hasta que se llama a este método, operaciones como Add, Update
    // o Delete pueden estar solamente registradas en el contexto de EF.
    //
    // Devuelve la cantidad de registros afectados.
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    // Inicia una transacción explícita en la base de datos.
    //
    // Es útil cuando una operación necesita realizar varios pasos
    // y queremos asegurarnos de que todos se completen correctamente
    // o que ninguno quede guardado.
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    // Confirma la transacción actual.
    //
    // A partir de este momento, los cambios realizados dentro de la
    // transacción quedan guardados de forma definitiva.
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    // Revierte la transacción actual.
    //
    // Se utiliza cuando ocurre un error y necesitamos deshacer los
    // cambios realizados durante la operación.
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}