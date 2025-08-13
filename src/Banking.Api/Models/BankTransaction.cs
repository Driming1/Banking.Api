namespace Banking.Api.Models;

public enum TransactionType
{
    Deposit,
    Withdrawal,
    Transfer
}

public class BankTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public Guid? FromAccountId { get; set; }
    public Guid? ToAccountId { get; set; }
    public Guid? AccountId { get; set; } // For non-transfer ops
}
