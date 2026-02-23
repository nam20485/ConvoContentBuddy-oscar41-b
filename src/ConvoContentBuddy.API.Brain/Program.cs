var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults (OpenTelemetry, health checks, resilience)
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddSignalR();

// Add controllers
builder.Services.AddControllers();

var app = builder.Build();

// Map default health check endpoints
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
