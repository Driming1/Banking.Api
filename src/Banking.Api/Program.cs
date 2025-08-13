using Banking.Api.Data;
using Banking.Api.Endpoints;
using Banking.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddDbContext<BankingDbContext>(opt =>
{
    // In-memory DB for simplicity of the assignment
    opt.UseInMemoryDatabase("BankingDb");
});

builder.Services.AddScoped<IAccountService, AccountService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.MapGroup("/api")
   .WithTags("Banking API")
   .MapAccountEndpoints();

app.Run();

// Make Program class public & partial for WebApplicationFactory in tests.
public partial class Program { }
