# Banking Platform API (.NET 10)

API de banca digital: clientes, cuentas, saldo, depósitos/retiros e historial.

## Arquitectura (Clean Architecture)

```
Banking.Api            -> Controllers + GlobalExceptionHandler + Program (composition root)
Banking.Application    -> Servicios de aplicación, DTOs, interfaces (repositorios, UoW, generador)
Banking.Domain         -> Entidades (reglas de negocio), enums, excepciones de negocio
Banking.Infrastructure -> EF Core + SQLite, configuraciones, repositorios
Banking.Tests          -> xUnit
```

Dependencias: `Api -> Application -> Domain` e `Infrastructure -> Application -> Domain`.
Los controladores solo conocen `IAccountService` / `ICustomerService` y DTOs (sin usar `DbContext`).

## Ejecución

Requisitos: [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/Banking.Api      # http://localhost:5080
```

## Endpoints

| Método | Ruta | Descripción |
|---|---|---|
| POST | `/api/customers` | Crear cliente |
| GET | `/api/customers/{id}` | Consultar cliente |
| POST | `/api/accounts` | Crear cuenta `{ customerId, initialBalance }` |
| GET | `/api/accounts/{accountNumber}/balance` | Saldo |
| POST | `/api/accounts/{accountNumber}/deposits` | Depósito `{ amount }` |
| POST | `/api/accounts/{accountNumber}/withdrawals` | Retiro `{ amount }` |
| GET | `/api/accounts/{accountNumber}/transactions` | Historial cronológico |

`gender`: `Male`, `Female`.

## Decisiones de diseño

- **Número de cuenta**: `ACC-YyyyMMdd-XXXX` (fecha UTC + 4 dígitos con `RandomNumberGenerator`).
  Solo hay 10.000 combinaciones por día, por eso el servicio reintenta ante colisión y el
  **índice único** sobre `AccountNumber` es la última barrera.
- **Errores**: el dominio lanza `BusinessException` (`NotFoundException` -> 404, resto -> 400);
  `GlobalExceptionHandler` (`IExceptionHandler`) los convierte a `ProblemDetails`. Errores no
  controlados -> 500 con mensaje genérico (el detalle solo va al log).
- **Reglas de saldo** dentro de `BankAccount` (aggregate root). El saldo inicial se registra como depósito.
- Montos: `decimal`, > 0, máximo 2 decimales. Reloj inyectable (`TimeProvider`) para pruebas deterministas.
