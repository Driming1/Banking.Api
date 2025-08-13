using System.ComponentModel.DataAnnotations;

namespace Banking.Api.DTOs;

public record DepositRequest([property: Range(0.01, double.MaxValue)] decimal Amount);
