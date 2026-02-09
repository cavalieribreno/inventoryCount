using DotNetEnv;
using Csinv.Products.Repository;
using Csinv.Products.DTOs;
using Csinv.Products.Interfaces;
using Csinv.Products.Service;

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
builder.Services.AddScoped<IProductsRepository, ProductsRepository>();
builder.Services.AddScoped<IProductsService, ProductsService>();

var app = builder.Build();
app.UseCors("AllowFrontend");
app.MapControllers();
app.Run();

