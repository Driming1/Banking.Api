using Banking.Api.Models;

namespace Banking.Api.DTOs;

public record AccountResponse(Guid Id, string AccountNumber, decimal Balance, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt)
{
    public static AccountResponse From(Account a) => new(a.Id, a.AccountNumber, a.Balance, a.CreatedAt, a.UpdatedAt);
}
