using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using ProductService.Infrastructure.Persistence;
using ProductService.Infrastructure.Persistence.Repository;
using ProductService.Services;
using ProductService.Domain.Entities;
using ProductService.Service.Abstraction.Models;

namespace ProductService.Tests
{
    public class ProductServiceIntegrationTests
    {
        private readonly ProductDbContext _context;
        private readonly ProductService.Services.ProductService _service;

        public ProductServiceIntegrationTests()
        {
            
            var options = new DbContextOptionsBuilder<ProductDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{System.Guid.NewGuid()}")
                .Options;

            _context = new ProductDbContext(options);

            
            var productRepository = new ProductRepository(_context);
            var unitOfWork = new UnitOfWork(_context);

            
            _service = new ProductService.Services.ProductService(productRepository, unitOfWork);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList_WhenDatabaseIsEmpty()
        {
            
            var result = await _service.GetAllAsync(CancellationToken.None);

            
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task CreateAsync_ShouldAddProductToDatabase()
        {
            
            var request = new CreateProductRequest
            {
                Name = "Тестовый товар",
                Description = "Описание товара",
                Price = 999.99m,
                IsAvailable = true
            };
            int userId = 1;

            
            var result = await _service.CreateAsync(request, userId, CancellationToken.None);

            
            Assert.NotNull(result);
            Assert.Equal("Тестовый товар", result.Name);
            Assert.Equal(999.99m, result.Price);
            Assert.Equal(userId, result.UserId);

           
            var productsInDb = await _context.Products.ToListAsync();
            Assert.Single(productsInDb);
            Assert.Equal("Тестовый товар", productsInDb[0].Name);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnProduct_WhenExists()
        {
            
            var product = new Product
            {
                Name = "Товар для поиска",
                Description = "Описание",
                Price = 100m,
                IsAvailable = true,
                UserId = 1,
                IsHidden = false,
                CreatedAt = System.DateTime.UtcNow
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            
            var result = await _service.GetByIdAsync(product.Id, CancellationToken.None);

            
            Assert.NotNull(result);
            Assert.Equal("Товар для поиска", result.Name);
            Assert.Equal(product.Id, result.Id);
        }

        

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}