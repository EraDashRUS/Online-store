using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OnlineStore.Contracts;
using OnlineStore.Data;
using OnlineStore.Mappings;
using OnlineStore.Services;
using FluentValidation.AspNetCore;
using OnlineStore.DTOs.Validators;

var builder = WebApplication.CreateBuilder(args);

// Конфигурация базы данных
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Регистрация сервисов
builder.Services
    .AddScoped<IProductService, ProductService>()
    .AddScoped<ICartService, CartService>()
    .AddScoped(typeof(IRepository<>), typeof(Repository<>));

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// FluentValidation
builder.Services
    .AddFluentValidationAutoValidation()
    .AddValidatorsFromAssemblyContaining<ProductCreateDtoValidator>();

// Контроллеры
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "OnlineStore API",
        Version = "v1",
        Contact = new OpenApiContact { Name = "Developer" }
    });
});

var app = builder.Build();

// Конфигурация middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OnlineStore v1"));
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Logger.LogInformation("Application started on {Url}", app.Urls);
app.Run();