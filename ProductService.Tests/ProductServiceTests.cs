using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using ProductService.Domain.Entities;
using ProductService.Domain.Repository;
using ProductService.Service.Abstraction.Models;
using ProductService.Service;


namespace ProductService.Tests
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _mockRepo;
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly ProductService.Services.ProductService _service;

        public ProductServiceTests()
        {
            _mockRepo = new Mock<IProductRepository>();
            _mockUow = new Mock<IUnitOfWork>();
            _service = new ProductService.Services.ProductService(_mockRepo.Object, _mockUow.Object);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowUnauthorized_WhenUserIsNotOwner()
        {
            
            int productId = 1;
            int ownerId = 10;
            int attackerId = 99; 

            var product = new Product { Id = productId, UserId = ownerId, Name = "Old Name", Price = 100 };
            _mockRepo.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync(product);

            var request = new UpdateProductRequest { Name = "New Name" };

           
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.UpdateAsync(productId, request, attackerId, CancellationToken.None));

            Assert.Equal("Вы можете редактировать только свои продукты.", exception.Message);
            _mockRepo.Verify(r => r.Update(It.IsAny<Product>()), Times.Never); 
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateProduct_WhenUserIsOwner()
        {
            
            int productId = 1;
            int ownerId = 10;
            var product = new Product { Id = productId, UserId = ownerId, Name = "Old Name", Price = 100 };
            _mockRepo.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync(product);

            var request = new UpdateProductRequest { Name = "New Name" };

            
            await _service.UpdateAsync(productId, request, ownerId, CancellationToken.None);

            
            Assert.Equal("New Name", product.Name);
            _mockRepo.Verify(r => r.Update(product), Times.Once);
            _mockUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}