using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OnlineStore.Data;
using FluentValidation.AspNetCore;
using FluentValidation;
using OnlineStore.BusinessLogic.StaticLogic.Contracts;
using OnlineStore.BusinessLogic.StaticLogic.Validators;
using OnlineStore.BusinessLogic.DynamicLogic.Services;

var builder = WebApplication.CreateBuilder(args);

// ������������ ���� ������
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ����������� ��������
builder.Services
    .AddScoped<IProductService, ProductService>()
    .AddScoped<ICartService, CartService>()
    .AddScoped(typeof(IRepository<>), typeof(Repository<>))
    .AddScoped<IUserService, UserService>();


// ��������� FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<ProductCreateDtoValidator>(); // ���������� �������� ������

// �����������
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

// ������������ middleware
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