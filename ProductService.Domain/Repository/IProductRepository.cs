using ProductService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Domain.Repository
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync(CancellationToken ct = default);
        Task<Product?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<IEnumerable<Product>> GetByUserIdAsync(int userId, CancellationToken ct = default);
        Task<IEnumerable<Product>> SearchAsync(string? name, decimal? minPrice, decimal? maxPrice, bool? isAvailable, CancellationToken ct = default);
        Task AddAsync(Product product, CancellationToken ct = default);
        void Update(Product product);
        void Delete(Product product);
    }
}
