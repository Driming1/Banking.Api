using System.Net;
using System.Net.Http.Json;
using Banking.Api.DTOs;
using Banking.Api;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using System.Threading.Tasks;

namespace Banking.Api.Tests;

public class ApiEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(_ => { });
    }

    [Fact]
    public async Task Create_And_Get_Account_Should_Succeed()
    {
        var client = _factory.CreateClient();

        var create = new CreateAccountRequest(100m, "ACC-999");
        var resp = await client.PostAsJsonAsync("/api/accounts", create);
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);

        var created = await resp.Content.ReadFromJsonAsync<AccountResponse>();
        Assert.NotNull(created);

        var get = await client.GetAsync($"/api/accounts/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);

        var fetched = await get.Content.ReadFromJsonAsync<AccountResponse>();
        Assert.Equal(100m, fetched!.Balance);
    }

    [Fact]
    public async Task Withdraw_Insufficient_Should_Return_400()
    {
        var client = _factory.CreateClient();

        var create = new CreateAccountRequest(10m, "ACC-400");
        var resp = await client.PostAsJsonAsync("/api/accounts", create);
        var acc = await resp.Content.ReadFromJsonAsync<AccountResponse>();

        var bad = await client.PostAsJsonAsync($"/api/accounts/{acc!.Id}/withdraw", new WithdrawRequest(20m));
        Assert.Equal(HttpStatusCode.BadRequest, bad.StatusCode);
    }

    [Fact]
    public async Task Transfer_Should_Return_200()
    {
        var client = _factory.CreateClient();

        var a = await (await client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(100m, "A-1")))
            .Content.ReadFromJsonAsync<AccountResponse>();
        var b = await (await client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(0m, "B-1")))
            .Content.ReadFromJsonAsync<AccountResponse>();

        var xfer = new TransferRequest(a!.Id, b!.Id, 30m);
        var resp = await client.PostAsJsonAsync("/api/transfers", xfer);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task Get_By_Number_Should_Return_200()
    {
        var client = _factory.CreateClient();
        var accNum = "ACC-12345";
        await client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(50m, accNum));

        var resp = await client.GetAsync($"/api/accounts/by-number/{accNum}");

        resp.EnsureSuccessStatusCode();
        var acc = await resp.Content.ReadFromJsonAsync<AccountResponse>();
        Assert.NotNull(acc);
        Assert.Equal(accNum, acc!.AccountNumber);
        Assert.Equal(50m, acc.Balance);
    }

    [Fact]
    public async Task Get_By_Number_Unknown_Should_Return_404()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/api/accounts/by-number/NOPE");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

}
