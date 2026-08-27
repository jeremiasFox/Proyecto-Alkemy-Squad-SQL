using System.Linq.Expressions;

namespace DigitalArs.Domain.Interfaces;

// Define las operaciones básicas que cualquier repositorio de una entidad
// debería poder realizar.
//
// La idea es que el resto de la aplicación trabaje con esta interfaz sin
// necesitar saber si los datos vienen de Entity Framework, Dapper u otra
// tecnología de acceso a datos.
public interface IRepository<T> where T : class
{
    // Busca una entidad por su Id.
    //
    // Si no existe un registro con ese Id, devuelve null.
    Task<T?> GetByIdAsync(int id);

    // Obtiene todos los registros de la entidad.
    //
    // Si no existen registros, devuelve una colección vacía.
    Task<IEnumerable<T>> GetAllAsync();

    // Busca entidades que cumplan la condición indicada.
    //
    // Por ejemplo, se puede utilizar para buscar usuarios por email
    // o cuentas que cumplan alguna determinada condición.
    //
    // Expression permite que la implementación pueda convertir esta
    // condición en una consulta para la base de datos.
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

    // Agrega una nueva entidad al contexto.
    //
    // La inserción en la base de datos se realizará cuando se confirme
    // la operación mediante el Unit of Work.
    Task AddAsync(T entity);

    // Marca una entidad existente como modificada.
    //
    // Los cambios se guardarán en la base de datos cuando se confirme
    // la operación mediante el Unit of Work.
    void Update(T entity);

    // Marca una entidad para ser eliminada.
    //
    // La eliminación se ejecutará en la base de datos cuando se confirme
    // la operación mediante el Unit of Work.
    void Delete(T entity);
}