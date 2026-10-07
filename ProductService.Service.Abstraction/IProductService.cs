using ProductService.Service.Abstraction.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Service.Abstraction
{
    public interface IProductService
    {

        Task<IEnumerable<ProductDTO>> GetAllAsync(CancellationToken ct = default);
        Task<ProductDTO?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<IEnumerable<ProductDTO>> SearchAsync(string? name, decimal? minPrice, decimal? maxPrice, bool? isAvailable, CancellationToken ct = default);
        Task<ProductDTO> CreateAsync(CreateProductRequest request, int userId, CancellationToken ct = default);
        Task UpdateAsync(int id, UpdateProductRequest request, int userId, CancellationToken ct = default);
        Task DeleteAsync(int id, int userId, CancellationToken ct = default);
        Task HideProductsByUserIdAsync(int userId, CancellationToken ct = default);
        Task ShowProductsByUserIdAsync(int userId, CancellationToken ct = default);
    }
}
