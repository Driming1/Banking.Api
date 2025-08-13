using System.ComponentModel.DataAnnotations;

namespace Banking.Api.DTOs;

public record CreateAccountRequest(
    [property: Range(0, double.MaxValue, ErrorMessage = "Initial balance must be >= 0")] decimal InitialBalance,
    [property: Required, MaxLength(32)] string AccountNumber
);
