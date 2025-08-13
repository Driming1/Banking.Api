using Banking.Api.DTOs;
using Banking.Api.Models;
using Banking.Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Net.Mime;

namespace Banking.Api.Endpoints;

public static class AccountsEndpoints
{
    public static RouteGroupBuilder MapAccountEndpoints(this RouteGroupBuilder group)
    {
        var accounts = group.MapGroup("/accounts").WithOpenApi();

        // Create
        accounts.MapPost("/", async Task<Results<Created<AccountResponse>, BadRequest<string>, Conflict<string>>>
            (CreateAccountRequest req, IAccountService svc, CancellationToken ct) =>
        {
            try
            {
                var acc = await svc.CreateAccountAsync(req.AccountNumber, req.InitialBalance, ct);
                return TypedResults.Created($"/api/accounts/{acc.Id}", AccountResponse.From(acc));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
            {
                return TypedResults.Conflict(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return TypedResults.BadRequest(ex.Message);
            }
        })
        .Produces<AccountResponse>(StatusCodes.Status201Created, contentType: MediaTypeNames.Application.Json)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict)
        .WithName("CreateAccount");

        // Get by id
        accounts.MapGet("/{id:guid}", async Task<Results<Ok<AccountResponse>, NotFound>> (Guid id, IAccountService svc, CancellationToken ct) =>
        {
            var acc = await svc.GetAccountAsync(id, ct);
            return acc is null ? TypedResults.NotFound() : TypedResults.Ok(AccountResponse.From(acc));
        })
        .Produces<AccountResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .WithName("GetAccount");

        // List
        accounts.MapGet("/", async Task<Ok<List<AccountResponse>>> (IAccountService svc, CancellationToken ct) =>
        {
            var list = (await svc.GetAccountsAsync(ct)).Select(AccountResponse.From).ToList();
            return TypedResults.Ok(list);
        })
        .Produces<List<AccountResponse>>(StatusCodes.Status200OK)
        .WithName("ListAccounts");

        // Deposit
        accounts.MapPost("/{id:guid}/deposit", async (Guid id, DepositRequest req, IAccountService svc, CancellationToken ct) =>
        {
            try
            {
                var balance = await svc.DepositAsync(id, req.Amount, ct);
                return Results.Ok(new { accountId = id, balance });
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .WithName("Deposit");

        // Withdraw
        accounts.MapPost("/{id:guid}/withdraw", async (Guid id, WithdrawRequest req, IAccountService svc, CancellationToken ct) =>
        {
            try
            {
                var balance = await svc.WithdrawAsync(id, req.Amount, ct);
                return Results.Ok(new { accountId = id, balance });
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Insufficient funds"))
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .WithName("Withdraw");

        // Transfer
        group.MapPost("/transfers", async (TransferRequest req, IAccountService svc, CancellationToken ct) =>
        {
            try
            {
                var (fromBal, toBal) = await svc.TransferAsync(req.FromAccountId, req.ToAccountId, req.Amount, ct);
                return Results.Ok(new
                {
                    fromAccountId = req.FromAccountId,
                    toAccountId = req.ToAccountId,
                    fromBalance = fromBal,
                    toBalance = toBal
                });
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (ArgumentOutOfRangeException ex) // <= amount <= 0
            {
                return Results.BadRequest(ex.Message);
            }
            catch (ArgumentException ex) // <= transfer to self
            {
                return Results.BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Insufficient funds"))
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .WithName("Transfer");

        accounts.MapGet("/by-number/{accountNumber}",
        async (string accountNumber, IAccountService svc, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(accountNumber))
                return Results.BadRequest("Account number is required.");

            var account = await svc.GetAccountByNumberAsync(accountNumber, ct);
            return account is null ? Results.NotFound() : Results.Ok(AccountResponse.From(account));
        })
        .Produces<AccountResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .WithName("GetAccountByNumber")
        .WithOpenApi();

        return group;
    }
}
