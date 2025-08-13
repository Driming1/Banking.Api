using System.ComponentModel.DataAnnotations;

namespace Banking.Api.DTOs;

public record TransferRequest(
    [property: Required] Guid FromAccountId,
    [property: Required] Guid ToAccountId,
    [property: Range(0.01, double.MaxValue)] decimal Amount
);
