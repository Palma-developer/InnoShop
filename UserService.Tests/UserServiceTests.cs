using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using UserService.Domain.Repository;
using UserService.Service.Abstraction.Models;
using UserService.Domain.Entities;
using UserService.Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace UserService.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IRepositoryManager> _mockRepoManager;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly UserService.Service.UserService _service;
        private readonly Mock<IConfiguration> _mockConfiguration;

        public UserServiceTests()
        {
            _mockRepoManager = new Mock<IRepositoryManager>();
            _mockUserRepo = new Mock<IUserRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockConfiguration=new Mock<IConfiguration>();

            _mockRepoManager.Setup(r => r.UserRepository).Returns(_mockUserRepo.Object);
            _mockRepoManager.Setup(r => r.UnitOfWork).Returns(_mockUnitOfWork.Object);
            
            _service = new UserService.Service.UserService(_mockRepoManager.Object, _mockConfiguration.Object);
        }

        #region Тесты регистрации (CreateAsync)

       
        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenEmailAlreadyExists()
        {
            
            var newUser = new UserForCreate
            {
                Name = "Test",
                Email = "test@test.com",
                Password = "123",
                Role = "User" 
            };

            _mockUserRepo.Setup(r => r.GetByEmail(newUser.Email, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new User { Email = new Domain.ValueObject.Email("test@test.com") });

            
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(newUser));
            Assert.Contains("уже зарегистрирован", exception.Message);
        }

        #endregion

        #region Тесты авторизации (LoginAsync)

        [Fact]
        public async Task LoginAsync_ShouldThrowException_WhenEmailIsNotConfirmed()
        {
            
            var loginRequest = new UserForLogin { Email = "test@test.com", Password = "123" };
            var user = new User
            {
                Email = new Domain.ValueObject.Email("test@test.com"),
                Password = "123",
                EmailConfirmed = false
            };

            _mockUserRepo.Setup(r => r.GetByEmail(loginRequest.Email, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(user);

            
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.LoginAsync(loginRequest));
            Assert.Equal("Пользователь не подтвердил свою почту", exception.Message);
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowException_WhenPasswordIsWrong()
        {
            
            var loginRequest = new UserForLogin { Email = "test@test.com", Password = "wrong_password" };
            var user = new User
            {
                Email = new Domain.ValueObject.Email("test@test.com"),
                Password = "correct_password",
                EmailConfirmed = true
            };

            _mockUserRepo.Setup(r => r.GetByEmail(loginRequest.Email, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(user);

            
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.LoginAsync(loginRequest));
            Assert.Equal("Uncorrect password", exception.Message);
        }

        #endregion

        

        

       

        [Fact]
        public async Task ResetPasswordAsync_ShouldThrowException_WhenTokenIsExpired()
        {
            
            int userId = 1;
            string token = "valid-token";
            var user = new User
            {
                Id = userId,
                PasswordResetToken = token,
                PasswordResetTokenExprice = DateTime.UtcNow.AddMinutes(-10)
            };
            _mockUserRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.ResetPasswordAsync(userId, token, "newPassword123"));

            Assert.Contains("Срок действия ссылки истек", exception.Message);
        }

        
    }
}