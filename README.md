# Banking Solution (C# / .NET 8, Minimal API)

A simple REST API for a banking application. Supports creating accounts, listing/fetching accounts, depositing, withdrawing, and transferring funds. Covered with automated tests.

## Tech Stack
- .NET 8 (ASP.NET Core Minimal API)
- EF Core InMemory (for quick local setup)
- Swagger (OpenAPI)
- xUnit + FluentAssertions + WebApplicationFactory

## How to Run

```bash
# 1) Ensure .NET 8 SDK is installed (https://dotnet.microsoft.com/)
# 2) Restore and run the API
cd src/Banking.Api
dotnet restore
dotnet run
```

The API will start (by default on `http://localhost:5000` / `https://localhost:5001` or similar). Open Swagger UI at `/swagger`.

## How to Test

```bash
cd tests/Banking.Api.Tests
dotnet test
```

## REST Endpoints

- `POST /api/accounts` — create account  
  body: `{ "accountNumber": "ACC-001", "initialBalance": 100.00 }`

- `GET /api/accounts/{id}` — get account by id

- `GET /api/accounts` — list all accounts

- `POST /api/accounts/{id}/deposit` — deposit funds  
  body: `{ "amount": 50.00 }`

- `POST /api/accounts/{id}/withdraw` — withdraw funds  
  body: `{ "amount": 25.00 }`

- `POST /api/transfers` — transfer funds  
  body: `{ "fromAccountId": "...", "toAccountId": "...", "amount": 10.00 }`

- `Get /api/accounts/by-number/{accountNumber}` — account by account number  

### Design Choices
- **Minimal API** keeps the code compact and easy to read for test purposes.
- **EF Core InMemory** allows running without a real database. Swap to SQL Server/PostgreSQL by changing the `DbContext` registration.
- **Validation**: basic validation is performed via DTO annotations and service checks (positive amounts, no overdraft, no duplicate account numbers).
- **Tests**: include both service-level and endpoint-level coverage for main flows and error cases.
