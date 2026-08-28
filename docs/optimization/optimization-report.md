# Reporte de Optimización — DigitalArs

**Etapa 1 — Modelo de datos y acceso a datos**  
Proyecto: Billetera Virtual  
Tecnología: .NET 10 · Entity Framework Core 10 · SQL Server

---

## 1. Optimizaciones implementadas

### 1.1 Índices estratégicos

Los índices son la optimización más importante a nivel de base de datos. Reducen el costo de una consulta de O(n) (full scan) a O(log n) (tree seek).

#### IX_Users_Email — UNIQUE

**Problema que resuelve:** el login busca un usuario por email en cada autenticación. Sin índice, SQL Server recorre toda la tabla `Users` fila por fila.

**Impacto:** con 10.000 usuarios registrados, un full scan evalúa ~10.000 filas. Con el índice, el seek encuentra la fila en ~13 pasos (log₂ de 10.000). A medida que la plataforma crece, la diferencia se vuelve exponencial.

**Beneficio adicional:** al ser `UNIQUE`, el índice también funciona como restricción de integridad. SQL Server rechaza duplicados a nivel de motor, independientemente de la validación de la capa de aplicación.

---

#### IX_Accounts_UserId — UNIQUE

**Problema que resuelve:** obtener la cuenta de un usuario es la operación más frecuente de la billetera (ver saldo, hacer transferencia, consultar historial). Sin índice es un full scan de `Accounts`.

**Impacto:** la unicidad garantiza la relación 1:1 entre `User` y `Account` a nivel de base de datos, evitando estados inconsistentes que un bug en la aplicación podría generar.

---

#### IX_Transactions_Date

**Problema que resuelve:** el historial de movimientos siempre se filtra y ordena por fecha. `Transactions` es la tabla de mayor crecimiento en el tiempo (cada operación genera al menos un registro).

**Impacto:** sin este índice, consultas como "movimientos del último mes" harían un full scan sobre millones de filas. Con el índice, SQL Server hace un range seek directo sobre el rango de fechas.

---

#### IX_Transactions_FromAccountId / IX_Transactions_ToAccountId

**Problema que resuelve:** generados automáticamente por EF Core sobre las claves foráneas. Aceleran la consulta "todos los movimientos de una cuenta" y los JOINs entre `Transactions` y `Accounts`.

**Por qué son necesarios ambos:** `Transactions` tiene dos FK apuntando a `Accounts`. Sin índice en cada una, obtener los movimientos de una cuenta requeriría un full scan buscando en dos columnas distintas.

---

### 1.2 AsNoTracking en consultas de lectura

**Qué es:** EF Core por defecto agrega cada entidad leída al change tracker, un diccionario en memoria que detecta cambios para generar los UPDATE automáticamente. Para consultas de solo lectura este costo es innecesario.

**Implementación:** el `GenericRepository` aplica `AsNoTracking()` en `GetAllAsync` y `FindAsync`.

```csharp
public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    => await _dbSet.AsNoTracking().Where(predicate).ToListAsync();
```

**Impacto:** entre un 10% y un 30% menos de uso de memoria por request, dependiendo del volumen de datos retornados. En endpoints de consulta con muchos registros (historial de transacciones, listado de usuarios) la diferencia es notable.

---

### 1.3 Inicialización lazy de repositorios en UnitOfWork

**Qué es:** los repositorios dentro de `UnitOfWork` se instancian solo cuando se acceden por primera vez, usando el operador `??=`.

```csharp
public IRepository<Account> Accounts
    => _accounts ??= new GenericRepository<Account>(_context);
```

**Impacto:** un servicio que solo usa `Users` no paga el costo de instanciar `Accounts`, `Roles` y `Transactions`. En una aplicación con muchos servicios especializados, esto reduce la presión sobre el garbage collector.

---

### 1.4 Parámetros SQL (evitar SQL injection y mejorar plan cache)

**Qué es:** EF Core nunca interpola valores directamente en el SQL. Siempre genera parámetros nombrados.

```sql
-- Lo que EF genera (correcto)
WHERE [u].[RoleId] = @__roleId_0

-- Lo que NO hace (vulnerable e ineficiente)
WHERE [u].[RoleId] = 1
```

**Impacto doble:**

- **Seguridad:** elimina por diseño el riesgo de SQL injection.
- **Rendimiento:** SQL Server cachea el plan de ejecución por la estructura de la query. Con parámetros, la misma consulta con distintos valores reutiliza el plan cacheado. Con valores hardcodeados, SQL Server compilaría un nuevo plan cada vez.

---

### 1.5 Reintentos automáticos en la conexión

**Qué es:** configurado en `InfrastructureServiceExtensions` con `EnableRetryOnFailure`.

```csharp
sqlOptions.EnableRetryOnFailure(
    maxRetryCount: 3,
    maxRetryDelay: TimeSpan.FromSeconds(5),
    errorNumbersToAdd: null);
```

**Impacto:** ante fallos transitorios de red o reinicios del servidor SQL, EF Core reintenta automáticamente hasta 3 veces con espera creciente. Evita que cortes de red breves (muy comunes en entornos cloud) generen errores visibles para el usuario.

---

### 1.6 decimal(18,2) en lugar de float para montos

**Problema que resuelve:** `float` y `double` representan números en binario, lo que genera errores de precisión en operaciones de suma y resta.

```
// Ejemplo del problema con float
0.1 + 0.2 = 0.30000000000000004
```

**Implementación:** todas las columnas de monto (`Balance` en `Accounts`, `Amount` en `Transactions`) usan `decimal(18,2)`.

**Impacto:** precisión exacta para cada operación monetaria. Crítico en una billetera virtual donde errores de centavos acumulados representan pérdidas reales.

---

### 1.7 Borrado en cascada deshabilitado

**Problema que resuelve:** `Transactions` tiene dos FK apuntando a `Accounts`. Si ambas tuviesen `CASCADE DELETE`, SQL Server lanzaría un error de múltiples rutas de cascada (`multiple cascade paths`).

**Implementación:** `OnDelete(DeleteBehavior.Restrict)` en todas las relaciones.

**Impacto adicional:** en una billetera virtual, eliminar una cuenta o un usuario nunca debe borrar el historial de transacciones. El `Restrict` fuerza a la capa de aplicación a manejar el ciclo de vida de los datos de forma explícita y controlada.

---

## 2. Áreas de mejora identificadas para etapas futuras

| Área                             | Descripción                                                                                                                                                                                         | Prioridad |
| -------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------- |
| Índice filtrado en Balance       | `CREATE INDEX IX_Accounts_ActiveBalance ON Accounts(Balance) WHERE Balance > 0` útil si la consulta de cuentas activas se vuelve frecuente                                                          | Media     |
| Paginación                       | Los métodos `GetAllAsync` y `FindAsync` retornan colecciones completas. Ante volúmenes grandes de `Transactions` se necesita `Skip/Take` con parámetros de página                                   | Alta      |
| Proyecciones (Select)            | Las consultas actuales retornan la entidad completa. Para endpoints de listado conviene proyectar solo los campos necesarios con `.Select()` para reducir el tráfico de red y la memoria            | Media     |
| Soft delete                      | No hay columna `IsDeleted` / `DeletedAt`. Implementar borrado lógico es necesario antes de exponer endpoints de eliminación, para mantener la trazabilidad del historial financiero                 | Alta      |
| Índice compuesto en Transactions | Para el historial de una cuenta ordenado por fecha: `CREATE INDEX IX_Transactions_Account_Date ON Transactions(FromAccountId, Date DESC)` combinaría los dos índices actuales en una sola operación | Baja      |

---

## 3. Resumen ejecutivo

La Etapa 1 establece una base de datos correctamente normalizada y con los índices necesarios para las operaciones centrales de la billetera virtual. Las decisiones de modelado (tipos de dato, restricciones, relaciones) priorizan la integridad de los datos financieros sobre la simplicidad de implementación.

El patrón Repository + Unit of Work desacopla la lógica de negocio del motor de persistencia, lo que permite optimizar o reemplazar la capa de datos en el futuro sin impacto en la capa de aplicación.

Las mejoras de mayor impacto para la siguiente etapa son la **paginación** en consultas de historial y la implementación de **soft delete**, ambas necesarias antes de exponer los endpoints a producción.
