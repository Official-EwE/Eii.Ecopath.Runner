using Eii.Ecopath.Runner.Services.Runtime;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

builder.Services.AddSingleton<ICoreService, CoreService>();
builder.Services.AddTransient<NodeService>();
builder.Services.AddTransient<EcopathModifierService>();
builder.Services.AddTransient<EcosimModifierService>();
builder.Services.AddTransient<EcospaceModifierService>();
builder.Services.AddTransient<EwEEngine>();

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
