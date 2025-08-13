using System.ComponentModel.DataAnnotations;

namespace Banking.Api.Models;

public class Account
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>A human-friendly account number. Unique within this app.</summary>
    [Required]
    [MaxLength(32)]
    public string AccountNumber { get; set; } = default!;

    /// <summary>Current balance in the account. Non-negative.</summary>
    [Range(0, double.MaxValue)]
    public decimal Balance { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
