using UserService.Service.Abstraction.Models;

namespace UserService.Service.Abstraction
{
    public interface IUserService
    {
        Task<IEnumerable<UserDTO>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<IEnumerable<UserDTO>> GetAllActiveAsync(CancellationToken cancellationToken = default);

        Task<UserDTO> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<UserDTO> CreateAsync(UserDTO userDTO, CancellationToken cancellationToken = default);
        Task UpdateAsync(int id, UserDTO userDTO, CancellationToken cancellationToken = default);

        Task DeleteAsync(int id, CancellationToken cancellationToken = default);

        //метод для логина
        Task<string> LoginAsync(string email, string password, CancellationToken cancellationToken = default);

        Task SendEmailAsync(string email, string message, CancellationToken cancellationToken= default);

        Task<bool> ConfirmEmailAsync(int id, string code, CancellationToken cancellationToken = default);

        Task SaveConfirmationCodeAsync(int id, string code, CancellationToken cancellationToken = default);
    }
}
