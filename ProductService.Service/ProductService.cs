using Mapster;
using ProductService.Service.Abstraction;
using ProductService.Service.Abstraction.Models;
using ProductService.Domain.Entities;
using ProductService.Domain.Repository;

namespace ProductService.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IProductRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<ProductDTO>> GetAllAsync(CancellationToken ct = default)
        {
            var products = await _repository.GetAllAsync(ct);
            return products.Adapt<IEnumerable<ProductDTO>>();
        }

        public async Task<ProductDTO?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var product = await _repository.GetByIdAsync(id, ct);
            return product?.Adapt<ProductDTO>();
        }

        public async Task<IEnumerable<ProductDTO>> SearchAsync(
            string? name, decimal? minPrice, decimal? maxPrice, bool? isAvailable, CancellationToken ct = default)
        {
            var products = await _repository.SearchAsync(name, minPrice, maxPrice, isAvailable, ct);
            return products.Adapt<IEnumerable<ProductDTO>>();
        }

        public async Task<ProductDTO> CreateAsync(CreateProductRequest request, int userId, CancellationToken ct = default)
        {
            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                IsAvailable = request.IsAvailable,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(product, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return product.Adapt<ProductDTO>();
        }

        public async Task UpdateAsync(int id, UpdateProductRequest request, int userId, CancellationToken ct = default)
        {
            var product = await _repository.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"Продукт с ID {id} не найден.");

            if (product.UserId != userId)
                throw new UnauthorizedAccessException("Вы можете редактировать только свои продукты.");

            if (request.Name != null) product.Name = request.Name;
            if (request.Description != null) product.Description = request.Description;
            if (request.Price.HasValue) product.Price = request.Price.Value;
            if (request.IsAvailable.HasValue) product.IsAvailable = request.IsAvailable.Value;

            _repository.Update(product);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, int userId, CancellationToken ct = default)
        {
            var product = await _repository.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"Продукт с ID {id} не найден.");

            if (product.UserId != userId)
                throw new UnauthorizedAccessException("Вы можете удалять только свои продукты.");

            _repository.Delete(product);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        // Для интеграции с UserService (деактивация/активация)
        public async Task HideProductsByUserIdAsync(int userId, CancellationToken ct = default)
        {
            // Игнорируем глобальный фильтр, чтобы найти скрытые продукты тоже
            var products = await _repository.GetByUserIdAsync(userId, ct);
            foreach (var product in products)
            {
                product.IsHidden = true;
                _repository.Update(product);
            }
            await _unitOfWork.SaveChangesAsync(ct);
        }

        public async Task ShowProductsByUserIdAsync(int userId, CancellationToken ct = default)
        {
            var products = await _repository.GetByUserIdAsync(userId, ct);
            foreach (var product in products)
            {
                product.IsHidden = false;
                _repository.Update(product);
            }
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}