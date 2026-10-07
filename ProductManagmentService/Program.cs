using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProductService.Service;
using ProductService.Service.Abstraction;
using ProductService.Domain.Repository;
using ProductService.Infrastructure.Persistence;
using ProductService.Infrastructure.Persistence.Repository;
using ProductService.Infrastructure.Presentation.Middleware;
using ProductServiceImpl = ProductService.Services.ProductService;
using ProductService.Infrastructure.Presentation.Controllers;
using System.Reflection.Metadata; // Для AssemblyReference

namespace ProductService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Регистрация Mapster (если ты его настроил в ProductService, иначе закомментируй)
            // MapsterConfig.Register();

            // 2. Регистрация контроллеров с указанием сборки (требует наличия класса AssemblyReference)
            builder.Services.AddControllers().AddApplicationPart(typeof(ProductController).Assembly);


            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });
            // 3. Детальная настройка Swagger с поддержкой Bearer токена (как в UserService)
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ProductService API", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Введите JWT токен: Bearer {token}",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
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
                        new string[] {}
                    }
                });
            });

            // 4. Регистрация зависимостей (DI)
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            // Используем псевдоним ProductServiceImpl, чтобы компилятор не путал класс с неймспейсом
            builder.Services.AddScoped<IProductService, ProductServiceImpl>();

            // 5. Настройка базы данных (AddDbContextPool как в UserService)
            builder.Services.AddDbContextPool<ProductDbContext>(options =>
            {
                // Убедись, что в appsettings.json ключ называется "DefaultConnection" или "Database"
                var connectionString = builder.Configuration.GetConnectionString("Database");
                options.UseSqlServer(connectionString);
            });

            // 6. Аутентификация и Авторизация (JWT)
            builder.Services.AddAuthorization();
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = AuthOptions.ISSUER,       // Должно совпадать с UserService
                    ValidateAudience = true,
                    ValidAudience = AuthOptions.AUDIENCE,   // Должно совпадать с UserService
                    ValidateLifetime = true,
                    IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(), // Должен быть тот же ключ
                    ValidateIssuerSigningKey = true,
                };

                options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        // Эта строка напишет в консоль ТОЧНУЮ причину ошибки (например, "Signature validation failed")
                        Console.WriteLine($"[JWT ОШИБКА] {context.Exception.Message}");
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Append("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine("[JWT УСПЕХ] Токен успешно проверен!");
                        return Task.CompletedTask;
                    }
                };
            });

            var app = builder.Build();

            // 7. Middleware pipeline (в том же порядке, что и в UserService)
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseMiddleware<ExceptionMiddleware>();
            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // 8. Автоматическая миграция БД при запуске
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
                db.Database.Migrate();
            }

            app.Run();
        }
    }
}