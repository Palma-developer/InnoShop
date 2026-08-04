    using Microsoft.EntityFrameworkCore;
    using UserService.Domain.Entities;

    namespace UserService.Infrastructure.Persistence
    {
        public sealed class RepositoryDbContext:DbContext
        {
            public RepositoryDbContext(DbContextOptions options) 
                :base(options)
            {

            }
            public DbSet<User> Users { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder) =>
                modelBuilder.ApplyConfigurationsFromAssembly(typeof(RepositoryDbContext).Assembly);

        }
    }
