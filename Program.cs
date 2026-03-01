using DotNetEnv;
using Csinv.InventoryProducts.Repository;
using Csinv.InventoryProducts.Interfaces;
using Csinv.InventoryProducts.Service;
using Csinv.InventorySessions.Interfaces;
using Csinv.InventorySessions.Service;
using Csinv.InventorySessions.Repository;

Env.Load("backend/.env");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(Environment.GetEnvironmentVariable("FRONTEND_URL"))
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
builder.Services.AddScoped<IInventoryProductsRepository, InventoryProductsRepository>();
builder.Services.AddScoped<IInventoryProductsService, InventoryProductsService>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<ISessionService, SessionService>();

var app = builder.Build();
app.UseCors("AllowFrontend");
app.MapControllers();
app.Run();

