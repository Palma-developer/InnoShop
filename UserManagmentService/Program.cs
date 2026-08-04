
using UserService.Infrastructure.Presintation;
using Microsoft.OpenApi.Models;
using UserService.Domain.Repository;
using UserService.Service.Abstraction;
using UserService.Service;
using UserService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using UserService.Infrastructure.Persistence.Repository;

namespace UserManagmentService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            MapsterConfig.Register();


            builder.Services.AddControllers().AddApplicationPart(typeof(UserService.Infrastructure.Presintation.AssemblyReference).Assembly);

            builder.Services.AddSwaggerGen(c =>
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Web", Version = "v1" }));

            builder.Services.AddScoped<IServiceManager, ServiceManager>();
            
            builder.Services.AddScoped<IRepositoryManager, RepositoryManager>();

            builder.Services.AddDbContextPool<RepositoryDbContext>(option=>
            {
                var connectionString = builder.Configuration.GetConnectionString("Database");
                option.UseSqlServer(connectionString);
            }); 

            

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

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
