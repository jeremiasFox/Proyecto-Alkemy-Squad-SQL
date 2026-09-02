using System.Linq.Expressions;

using DigitalArs.Domain.Interfaces;

using DigitalArs.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace DigitalArs.Infrastructure.Repositories;

// Implementación genérica de IRepository<T> utilizando Entity Framework Core.
//
// La idea de este repositorio es evitar tener que repetir las mismas operaciones
// de acceso a datos para cada entidad. El mismo repositorio puede trabajar con
// User, Account, Transaction, Role, etc.
public class GenericRepository<T> : IRepository<T> where T : class
{
    // DbContext es el objeto que utiliza Entity Framework para comunicarse
    // con la base de datos y llevar el seguimiento de las entidades.
    //
    // Lo recibimos por inyección de dependencias para utilizar el mismo
    // contexto que utiliza el UnitOfWork.
    protected readonly AppDbContext _context;

    // DbSet<T> representa la colección de registros de una determinada entidad.
    //
    // Por ejemplo, cuando T es User, _dbSet representa los registros de Users.
    protected readonly DbSet<T> _dbSet;

    // Recibimos el DbContext desde el contenedor de dependencias.
    //
    // A partir de este contexto obtenemos el DbSet correspondiente a T,
    // lo que permite que este repositorio funcione con cualquier entidad.
    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    // Busca una entidad por su Id.
    //
    // FindAsync tiene la particularidad de que primero revisa si la entidad
    // ya está siendo seguida por el DbContext. Si no la encuentra ahí,
    // consulta la base de datos.
    public async Task<T?> GetByIdAsync(int id)
        => await _dbSet.FindAsync(id);

    // Obtiene todos los registros de la entidad.
    //
    // Usamos AsNoTracking porque solamente queremos leer los datos.
    // Al no necesitar modificar estas entidades, evitamos que EF Core
    // tenga que mantenerlas bajo seguimiento.
    public async Task<IEnumerable<T>> GetAllAsync()
        => await _dbSet.AsNoTracking().ToListAsync();

    // Busca registros utilizando la condición recibida en predicate.
    //
    // La expresión se transforma en una consulta SQL y se ejecuta
    // directamente en la base de datos.
    //
    // AsNoTracking indica nuevamente que los resultados son solo para
    // lectura y que no necesitamos que EF Core siga sus cambios.
    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.AsNoTracking().Where(predicate).ToListAsync();

    // Agrega una nueva entidad al DbContext.
    //
    // En este punto todavía no se ejecuta necesariamente el INSERT en la BD.
    // EF Core deja la entidad marcada como Added y la inserción se realiza
    // cuando se llama a SaveChangesAsync().
    public async Task AddAsync(T entity)
        => await _dbSet.AddAsync(entity);

    // Marca la entidad como modificada.
    //
    // EF Core la deja en estado Modified y cuando se llame a
    // SaveChangesAsync() generará el UPDATE correspondiente.
    public void Update(T entity)
        => _dbSet.Update(entity);

    // Marca la entidad para eliminarla.
    //
    // El DELETE se ejecutará realmente en la base de datos cuando
    // se llame a SaveChangesAsync().
    public void Delete(T entity)
        => _dbSet.Remove(entity);
}