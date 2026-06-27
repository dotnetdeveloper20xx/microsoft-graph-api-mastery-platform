using GraphBridge.Api.Middleware;
using GraphBridge.Application;
using GraphBridge.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog structured logging
builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console());

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Application layer services (MediatR, FluentValidation, pipeline behaviors)
builder.Services.AddApplication();

// Determine Graph mode from configuration and log startup mode
var graphMode = builder.Configuration["GraphBridge:GraphMode"];
var validModes = new[] { "Live", "Demo" };

if (string.IsNullOrWhiteSpace(graphMode) || !validModes.Contains(graphMode, StringComparer.OrdinalIgnoreCase))
{
    if (!string.IsNullOrWhiteSpace(graphMode))
    {
        Log.Warning("Unrecognised GraphBridge:GraphMode value '{GraphMode}'. Defaulting to Demo mode", graphMode);
    }
    else
    {
        Log.Warning("GraphBridge:GraphMode configuration is absent. Defaulting to Demo mode");
    }
}

if (string.Equals(graphMode, "Live", StringComparison.OrdinalIgnoreCase))
{
    Log.Information("GraphBridge starting in Live mode — real Microsoft Graph SDK services will be used");
}
else
{
    Log.Information("GraphBridge starting in Demo mode — mock Graph services will be used");
}

// Register Infrastructure layer services (mock or live Graph implementations + in-memory stores)
builder.Services.AddGraphBridgeInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware pipeline order: CorrelationId → GlobalException → Routing → Controllers
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseRouting();
app.MapControllers();

app.Run();

// Make the implicit Program class public so test projects can access it
public partial class Program { }
