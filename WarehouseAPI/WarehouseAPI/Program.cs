using Microsoft.EntityFrameworkCore;
using WarehouseAPI.Data;
using WarehouseAPI.Managers;
using WarehouseAPI.Managers.Interfaces;
using WarehouseAPI.Repositories;
using WarehouseAPI.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// CORS — frontend'e izin ver
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();
builder.Services.AddScoped<IStockTransactionRepository, StockTransactionRepository>();

// Managers
builder.Services.AddScoped<IProductManager, ProductManager>();
builder.Services.AddScoped<IWarehouseManager, WarehouseManager>();
builder.Services.AddScoped<IStockTransactionManager, StockTransactionManager>();
builder.Services.AddScoped<IDashboardManager, DashboardManager>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

app.Run();
