using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using FluentValidation.AspNetCore;
using FluentValidation;
using OnlineStore.BusinessLogic.StaticLogic.Contracts;
using OnlineStore.BusinessLogic.StaticLogic.Validators;
using OnlineStore.BusinessLogic.DynamicLogic.Services;
using System.Text.Json.Serialization;
using OnlineStore.BusinessLogic.StaticLogic.Settings;
using System.Security.Claims;
using OnlineStore.Storage.Data;
using OnlineStore.Storage.Models;
using OnlineStore.BusinessLogic.DynamicLogic.UseCases;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer; // Для JWT-аутентификации
using Microsoft.IdentityModel.Tokens; // Для работы с токенами
using System.Text; // Для кодирования ключа

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
    .AddScoped<AdminEmailFilter>()
    .AddScoped<IAdminChecker, AdminChecker>()
    .AddScoped<IAdminOrderService, AdminOrderService>()
    .AddScoped<IAdminCommentService, AdminCommentService>()
    .AddHttpContextAccessor();

builder.Services.AddSingleton<IAdminCommentService, AdminCommentService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
    policy.RequireAuthenticatedUser()
          .RequireRole("Admin"));
});

builder.Services.Configure<AdminSettings>(builder.Configuration.GetSection("AdminSettings"));

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<ProductCreateDtoValidator>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddHttpClient();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Admin API", Version = "v1" });

    // Добавляем поддержку JWT в Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


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
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Logger.LogInformation("Application started on {Url}", app.Urls);
app.Run();