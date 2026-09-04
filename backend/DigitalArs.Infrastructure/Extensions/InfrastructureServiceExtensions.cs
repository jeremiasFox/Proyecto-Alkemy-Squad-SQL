using DigitalArs.Domain.Interfaces;

using DigitalArs.Infrastructure.Data;

using DigitalArs.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Configuration;

using Microsoft.Extensions.DependencyInjection;

namespace DigitalArs.Infrastructure.Extensions;

// Centralizamos acá el registro de todos los servicios relacionados
// con la infraestructura de la aplicación.
//
// De esta forma, Program.cs no necesita conocer cómo se configura
// Entity Framework, los repositorios o el Unit of Work.
// Solo tiene que llamar a AddInfrastructure().
public static class InfrastructureServiceExtensions
{
    // Registra todos los servicios que necesita la capa de Infrastructure
    // dentro del contenedor de inyección de dependencias.
    //
    // Los servicios se registran como Scoped, por lo que se crea una
    // instancia para cada request HTTP y se reutiliza durante ese request.
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Registramos el DbContext que utiliza Entity Framework para
        // comunicarse con la base de datos.
        //
        // AddDbContext lo registra como Scoped por defecto, por lo que
        // cada request tendrá su propio contexto.
        //
        // La cadena de conexión se obtiene desde la configuración,
        // normalmente desde appsettings.json.
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")));

        // Registramos la implementación de IUnitOfWork.
        //
        // Desde la capa de Application vamos a trabajar con IUnitOfWork
        // y no directamente con la implementación UnitOfWork.
        //
        // Scoped hace que el mismo UnitOfWork se mantenga durante todo
        // el request y pueda trabajar con el mismo DbContext.
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Registramos el repositorio genérico.
        //
        // Esto permite que .NET pueda crear automáticamente un
        // GenericRepository<T> cuando alguna clase solicite
        // IRepository<T> mediante inyección de dependencias.
        //
        // Por ejemplo:
        // IRepository<User> → GenericRepository<User>
        // IRepository<Account> → GenericRepository<Account>
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));

        return services;
    }
}