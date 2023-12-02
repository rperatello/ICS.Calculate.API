using ICS.Calculate.API.Util;
using Microsoft.AspNetCore.Builder;
using Newtonsoft.Json;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole(); // Adiciona o provedor de console para logging
});

var logger = loggerFactory.CreateLogger<Program>();


// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

#region Enviroment Variables

string collectorProtocol = Environment.GetEnvironmentVariable("COLLECTOR_PROTOCOL");
string collectorHost = Environment.GetEnvironmentVariable("COLLECTOR_HOST");
string collectorPort = Environment.GetEnvironmentVariable("COLLECTOR_PORT");

logger.LogInformation($"collectorProtocol informed: {collectorProtocol}");
logger.LogInformation($"collectorHost informed: {collectorHost}");
logger.LogInformation($"collectorPort informed: {collectorPort}");

ComnunicationSettings.CollectorHost = $"{(!String.IsNullOrWhiteSpace(collectorProtocol) ? collectorProtocol : "http")}://{(!String.IsNullOrWhiteSpace(collectorHost) ? collectorHost : "localhost")}" ;
ComnunicationSettings.CollectorPort = !String.IsNullOrWhiteSpace(collectorPort) ? int.Parse(collectorPort) : 5126;

logger.LogInformation($"CollectorHost: {ComnunicationSettings.CollectorHost}");
logger.LogInformation($"CollectorPort: {ComnunicationSettings.CollectorPort}");

#endregion

// Ativa o sistema de roteamento
app.UseRouting();

//app.UseHttpsRedirection();

// Ativa a política CORS configurada em ConfigureServices
app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.UseCors();

app.Run();
