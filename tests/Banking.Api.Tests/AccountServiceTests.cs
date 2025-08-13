using System;
using System.Threading;
using System.Threading.Tasks;
using Banking.Api.Data;
using Banking.Api.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Banking.Api.Tests;

public class AccountServiceTests
{
    private BankingDbContext NewDb()
    {
        var options = new DbContextOptionsBuilder<BankingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new BankingDbContext(options);
    }

    [Fact]
    public async Task Create_Then_Deposit_And_Withdraw_Should_Update_Balance()
    {
        await using var db = NewDb();
        var svc = new AccountService(db);

        var acc = await svc.CreateAccountAsync("ACC-001", 100m);
        (await svc.DepositAsync(acc.Id, 50m)).Should().Be(150m);
        (await svc.WithdrawAsync(acc.Id, 25m)).Should().Be(125m);
    }

    [Fact]
    public async Task Withdraw_With_Insufficient_Funds_Should_Fail()
    {
        await using var db = NewDb();
        var svc = new AccountService(db);

        var acc = await svc.CreateAccountAsync("ACC-001", 10m);
        Func<Task> act = async () => await svc.WithdrawAsync(acc.Id, 20m);
        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("*Insufficient funds*");
    }

    [Fact]
    public async Task Transfer_Should_Move_Funds()
    {
        await using var db = NewDb();
        var svc = new AccountService(db);

        var a = await svc.CreateAccountAsync("A", 100m);
        var b = await svc.CreateAccountAsync("B", 0m);

        var (fromBal, toBal) = await svc.TransferAsync(a.Id, b.Id, 60m);
        fromBal.Should().Be(40m);
        toBal.Should().Be(60m);
    }
}
