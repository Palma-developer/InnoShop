
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using UserService.Infrastructure.Presintation;
using Microsoft.OpenApi.Models;
using UserService.Domain.Repository;
using UserService.Service.Abstraction;
using UserService.Service;
using UserService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using UserService.Infrastructure.Persistence.Repository;
using Microsoft.IdentityModel.Tokens;

namespace UserManagmentService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

           
            MapsterConfig.Register();


            builder.Services.AddControllers().AddApplicationPart(typeof(UserService.Infrastructure.Presintation.AssemblyReference).Assembly);
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });


            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Web", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "¬ведите JWT токен: Bearer {token}",
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



            builder.Services.AddScoped<IServiceManager, ServiceManager>();
            
            builder.Services.AddScoped<IRepositoryManager, RepositoryManager>();

            builder.Services.AddDbContextPool<RepositoryDbContext>(option=>
            {
                var connectionString = builder.Configuration.GetConnectionString("Database");
                option.UseSqlServer(connectionString);
            });

            builder.Services.AddAuthorization();
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = AuthOptions.ISSUER,
                    ValidateAudience=true,
                    ValidAudience=AuthOptions.AUDIENCE,
                    ValidateLifetime=true,
                    IssuerSigningKey=AuthOptions.GetSymmetricSecurityKey(),
                    ValidateIssuerSigningKey=true,
                };
            });
            

            var app = builder.Build();

            

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();
            


            app.MapControllers();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<RepositoryDbContext>();
                db.Database.Migrate();
            }

            

            app.Run();
        }
    }
}
