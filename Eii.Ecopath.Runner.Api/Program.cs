using Eii.Ecopath.Runner.Services.Runtime;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

builder.Services.AddSingleton<ICoreService, cCoreService>();
builder.Services.AddTransient<cNodeService>();
builder.Services.AddTransient<cEcopathModifierService>();
builder.Services.AddTransient<cEcosimModifierService>();
builder.Services.AddTransient<cEcospaceModifierService>();
builder.Services.AddTransient<cEwEEngine>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/healthz");

app.Run();
