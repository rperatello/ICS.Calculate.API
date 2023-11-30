using ICS.Calculate.API.Util;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
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

string collectorHost = Environment.GetEnvironmentVariable("COLLECTOR_HOST");
string collectorPort = Environment.GetEnvironmentVariable("COLLECTOR_PORT");

ComnunicationSettings.CollectorHost = collectorHost ?? "localhost";
ComnunicationSettings.CollectorPort = !String.IsNullOrWhiteSpace(collectorPort) ? int.Parse(collectorPort) : 5126;

#endregion

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
