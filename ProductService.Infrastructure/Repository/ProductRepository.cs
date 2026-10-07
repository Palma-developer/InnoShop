using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Domain.Repository;

namespace ProductService.Infrastructure.Persistence.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly ProductDbContext _context;

        public ProductRepository(ProductDbContext context) => _context = context;

        public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Products.ToListAsync(ct);
        }

        public async Task<Product?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _context.Products.FirstOrDefaultAsync(p => p.Id == id, ct);
        }

        public async Task<IEnumerable<Product>> GetByUserIdAsync(int userId, CancellationToken ct = default)
        {
            return await _context.Products.Where(p => p.UserId == userId).ToListAsync(ct);
        }

        public async Task<IEnumerable<Product>> SearchAsync(
            string? name, decimal? minPrice, decimal? maxPrice, bool? isAvailable, CancellationToken ct = default)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(p => p.Name.Contains(name));
            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);
            if (isAvailable.HasValue)
                query = query.Where(p => p.IsAvailable == isAvailable.Value);

            return await query.ToListAsync(ct);
        }

        public async Task AddAsync(Product product, CancellationToken ct = default)
        {
            await _context.Products.AddAsync(product, ct);
        }

        public void Update(Product product)
        {
            _context.Products.Update(product);
        }

        public void Delete(Product product)
        {
            _context.Products.Remove(product);
        }
        public async Task<IEnumerable<Product>> GetAllByUserIdIgnoringFiltersAsync(int userId, CancellationToken ct = default)
        {
            return await _context.Products
                .IgnoreQueryFilters() // <-- ЭТА СТРОКА ОТКЛЮЧАЕТ ПРОВЕРКУ IsHidden = 0
                .Where(p => p.UserId == userId)
                .ToListAsync(ct);
        }
    }
}