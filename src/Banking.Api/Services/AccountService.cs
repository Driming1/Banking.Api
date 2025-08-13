using Banking.Api.Data;
using Banking.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Banking.Api.Services;

public class AccountService : IAccountService
{
    private readonly BankingDbContext _db;

    public AccountService(BankingDbContext db)
    {
        _db = db;
    }

    public async Task<Account> CreateAccountAsync(string accountNumber, decimal initialBalance, CancellationToken ct = default)
    {
        if (initialBalance < 0) throw new ArgumentOutOfRangeException(nameof(initialBalance));
        if (string.IsNullOrWhiteSpace(accountNumber)) throw new ArgumentException("Account number is required.", nameof(accountNumber));

        var exists = await _db.Accounts.AnyAsync(a => a.AccountNumber == accountNumber, ct);
        if (exists) throw new InvalidOperationException($"Account number '{accountNumber}' already exists.");

        var acc = new Account
        {
            AccountNumber = accountNumber,
            Balance = initialBalance
        };
        await _db.Accounts.AddAsync(acc, ct);

        if (initialBalance > 0)
        {
            await _db.Transactions.AddAsync(new BankTransaction
            {
                Type = TransactionType.Deposit,
                Amount = initialBalance,
                AccountId = acc.Id
            }, ct);
        }

        await _db.SaveChangesAsync(ct);
        return acc;
    }

    public Task<Account?> GetAccountAsync(Guid id, CancellationToken ct = default) =>
        _db.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<List<Account>> GetAccountsAsync(CancellationToken ct = default) =>
        _db.Accounts.AsNoTracking().OrderBy(a => a.CreatedAt).ToListAsync(ct);

    public async Task<decimal> DepositAsync(Guid accountId, decimal amount, CancellationToken ct = default)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));

        var acc = await _db.Accounts.FirstOrDefaultAsync(a => a.Id == accountId, ct)
                  ?? throw new KeyNotFoundException("Account not found.");

        acc.Balance += amount;
        acc.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.Transactions.AddAsync(new BankTransaction
        {
            Type = TransactionType.Deposit,
            Amount = amount,
            AccountId = acc.Id
        }, ct);

        await _db.SaveChangesAsync(ct);
        return acc.Balance;
    }

    public async Task<decimal> WithdrawAsync(Guid accountId, decimal amount, CancellationToken ct = default)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));

        var acc = await _db.Accounts.FirstOrDefaultAsync(a => a.Id == accountId, ct)
                  ?? throw new KeyNotFoundException("Account not found.");

        if (acc.Balance < amount) throw new InvalidOperationException("Insufficient funds.");

        acc.Balance -= amount;
        acc.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.Transactions.AddAsync(new BankTransaction
        {
            Type = TransactionType.Withdrawal,
            Amount = amount,
            AccountId = acc.Id
        }, ct);

        await _db.SaveChangesAsync(ct);
        return acc.Balance;
    }

    public async Task<(decimal fromBalance, decimal toBalance)> TransferAsync(Guid fromAccountId, Guid toAccountId, decimal amount, CancellationToken ct = default)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        if (fromAccountId == toAccountId) throw new ArgumentException("Cannot transfer to the same account.");

        var from = await _db.Accounts.FirstOrDefaultAsync(a => a.Id == fromAccountId, ct)
                   ?? throw new KeyNotFoundException("From account not found.");
        var to = await _db.Accounts.FirstOrDefaultAsync(a => a.Id == toAccountId, ct)
                 ?? throw new KeyNotFoundException("To account not found.");

        if (from.Balance < amount) throw new InvalidOperationException("Insufficient funds.");

        from.Balance -= amount;
        from.UpdatedAt = DateTimeOffset.UtcNow;
        to.Balance += amount;
        to.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.Transactions.AddAsync(new BankTransaction
        {
            Type = TransactionType.Transfer,
            Amount = amount,
            FromAccountId = from.Id,
            ToAccountId = to.Id
        }, ct);

        await _db.SaveChangesAsync(ct);
        return (from.Balance, to.Balance);
    }

    public async Task<Account?> GetAccountByNumberAsync(string accountNumber, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
            throw new ArgumentException("Account number is required.", nameof(accountNumber));

        return await _db.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber, ct);
    }

}
