using ProductService.Domain.Repository;

namespace ProductService.Infrastructure.Persistence.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ProductDbContext _context;
        public UnitOfWork(ProductDbContext context) => _context = context;

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }
    }
}