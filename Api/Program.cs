using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OnlineStore.Data;
using FluentValidation.AspNetCore;
using FluentValidation;
using OnlineStore.BusinessLogic.StaticLogic.Contracts;
using OnlineStore.BusinessLogic.StaticLogic.Validators;
using OnlineStore.BusinessLogic.DynamicLogic.Services;
using System.Text.Json.Serialization;
using OnlineStore.BusinessLogic.StaticLogic.Settings;
using OnlineStore.Models;
using System.Security.Claims;

/// <summary>
/// Точка входа в приложение
/// </summary>
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddScoped<IProductService, ProductService>()
    .AddScoped<ICartService, CartService>()
    .AddScoped(typeof(IRepository<>), typeof(Repository<>))
    .AddScoped<IUserService, UserService>()
    .AddScoped<IProductService, ProductService>()
    .AddScoped<IRepository<Product>, Repository<Product>>()
    .AddScoped<IOrderService, OrderService>()
    .AddScoped<AdminEmailFilter>();

builder.Services.Configure<AdminSettings>(builder.Configuration.GetSection("AdminSettings"));

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<ProductCreateDtoValidator>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddHttpClient();

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

builder.Services.AddAuthentication("DummyScheme")
    .AddCookie("DummyScheme", options => { });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OnlineStore v1"));
}

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        if (context.Request.Headers.TryGetValue("X-User-Email", out var email))
        {
            var claims = new[] { new Claim(ClaimTypes.Email, email!) };
            context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        }
        await next();
    });
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Logger.LogInformation("Application started on {Url}", app.Urls);
app.Run();