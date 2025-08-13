using Banking.Api.Models;

namespace Banking.Api.Services;

public interface IAccountService
{
    Task<Account> CreateAccountAsync(string accountNumber, decimal initialBalance, CancellationToken ct = default);
    Task<Account?> GetAccountAsync(Guid id, CancellationToken ct = default);
    Task<List<Account>> GetAccountsAsync(CancellationToken ct = default);
    Task<decimal> DepositAsync(Guid accountId, decimal amount, CancellationToken ct = default);
    Task<decimal> WithdrawAsync(Guid accountId, decimal amount, CancellationToken ct = default);
    Task<(decimal fromBalance, decimal toBalance)> TransferAsync(Guid fromAccountId, Guid toAccountId, decimal amount, CancellationToken ct = default);
    Task<Account?> GetAccountByNumberAsync(string accountNumber, CancellationToken ct = default);
}
