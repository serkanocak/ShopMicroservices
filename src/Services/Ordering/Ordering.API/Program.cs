using BuildingBlocks.Logging;
using Ordering.API;
using Ordering.Application;
using Ordering.Infrastructure;
using Ordering.Infrastructure.Data.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// --- Serilog + Elasticsearch Logging ---
builder.AddSerilogWithElasticsearch("Ordering.API");

// Add services to the container.
builder.Services
    .AddApplicationServices(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration)
    .AddApiServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseApiServices();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    await app.InitialiseDatabaseAsync();
}

app.Run();
