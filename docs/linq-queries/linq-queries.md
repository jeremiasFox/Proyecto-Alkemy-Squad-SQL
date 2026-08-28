# Consultas LINQ — DigitalArs

Documentación de las 4 consultas principales del dominio, con el código C# y el SQL equivalente que EF Core genera internamente.

Todas las consultas se ejecutan a través de `IUnitOfWork` desde la capa de aplicación. El método `FindAsync` usa `AsNoTracking` porque son operaciones de solo lectura, lo que mejora el rendimiento al no cargar el change tracker de EF Core.

---

## 1. Buscar usuario por email

### LINQ

```csharp
var usuarios = await _unitOfWork.Users
    .FindAsync(u => u.Email == "juan@email.com");

var usuario = usuarios.FirstOrDefault();
```

### SQL generado por EF Core

```sql
SELECT TOP(1) [u].[Id],
              [u].[Email],
              [u].[Name],
              [u].[Password],
              [u].[RoleId]
FROM [Users] AS [u]
WHERE [u].[Email] = N'juan@email.com'
```

### Por qué funciona eficientemente

La columna `Email` tiene el índice único `IX_Users_Email`. SQL Server usa ese índice para localizar la fila directamente sin recorrer la tabla completa, sin importar cuántos usuarios haya registrados.

---

## 2. Obtener usuarios de un rol

### LINQ

```csharp
var usuarios = await _unitOfWork.Users
    .FindAsync(u => u.RoleId == roleId);
```

### SQL generado por EF Core

```sql
SELECT [u].[Id],
       [u].[Email],
       [u].[Name],
       [u].[Password],
       [u].[RoleId]
FROM [Users] AS [u]
WHERE [u].[RoleId] = @__roleId_0
```

### Por qué funciona eficientemente

EF Core parametriza el valor del rol (`@__roleId_0`) en lugar de interpolarlo directamente, evitando SQL injection y permitiendo que SQL Server reutilice el plan de ejecución cacheado para consultas futuras con distinto `roleId`. El índice `IX_Users_RoleId` generado automáticamente por EF Core sobre la FK acelera el filtro.

---

## 3. Buscar cuentas con saldo mayor a 0

### LINQ

```csharp
var cuentas = await _unitOfWork.Accounts
    .FindAsync(a => a.Balance > 0);
```

### SQL generado por EF Core

```sql
SELECT [a].[Id],
       [a].[Balance],
       [a].[UserId]
FROM [Accounts] AS [a]
WHERE [a].[Balance] > 0.0
```

### Por qué funciona eficientemente

EF Core traduce el predicado `a.Balance > 0` directamente a una cláusula `WHERE` en SQL, empujando el filtro al motor de base de datos (server-side evaluation). Esto evita traer todas las filas a memoria para filtrarlas en C#. Para esta consulta no hay índice sobre `Balance` porque es un rango variable; si se volviera una consulta crítica y frecuente, se podría agregar un índice filtrado sobre `Balance > 0`.

---

## 4. Obtener movimientos de una cuenta

### LINQ

```csharp
// Todos los movimientos donde la cuenta participó (como origen o destino)
var movimientos = await _unitOfWork.Transactions
    .FindAsync(t => t.FromAccountId == accountId
                 || t.ToAccountId   == accountId);
```

### SQL generado por EF Core

```sql
SELECT [t].[Id],
       [t].[Amount],
       [t].[Date],
       [t].[FromAccountId],
       [t].[ToAccountId],
       [t].[TransactionType]
FROM [Transactions] AS [t]
WHERE [t].[FromAccountId] = @__accountId_0
   OR [t].[ToAccountId]   = @__accountId_0
```

### Por qué funciona eficientemente

Los índices `IX_Transactions_FromAccountId` e `IX_Transactions_ToAccountId` (generados automáticamente por EF Core sobre las FK) permiten que SQL Server resuelva cada rama del `OR` usando un Index Seek en lugar de un Full Scan. El motor combina los resultados de ambos índices con un operador `OR` en el plan de ejecución.

Si se necesita ordenar por fecha (historial cronológico), se puede extender la consulta:

```csharp
var movimientos = (await _unitOfWork.Transactions
    .FindAsync(t => t.FromAccountId == accountId
                 || t.ToAccountId   == accountId))
    .OrderByDescending(t => t.Date)
    .ToList();
```

En ese caso SQL Server también aprovecha el índice `IX_Transactions_Date` para el ordenamiento.

---

## Resumen de índices utilizados

| Consulta                  | Índice aprovechado                                                                     |
| ------------------------- | -------------------------------------------------------------------------------------- |
| Buscar por email          | `IX_Users_Email` (UNIQUE)                                                              |
| Usuarios por rol          | `IX_Users_RoleId`                                                                      |
| Cuentas con saldo > 0     | _(sin índice; candidato a índice filtrado si escala)_                                  |
| Movimientos de una cuenta | `IX_Transactions_FromAccountId`, `IX_Transactions_ToAccountId`, `IX_Transactions_Date` |
