using UserService.Service.Abstraction.Models;

namespace UserService.Service.Abstraction
{
    public interface IUserService
    {
        Task<IEnumerable<UserDTO>> GetAllAsync( CancellationToken cancellationToken = default);

        Task<UserDTO> GetByIdAsync(int id, CancellationToken cancellationToken= default);

        Task<UserDTO> CreateAsync(UserDTO userDTO, CancellationToken cancellationToken= default);
        Task UpdateAsync(int id, UserDTO userDTO, CancellationToken cancellationToken= default);

        Task DeleteAsync(int id, CancellationToken cancellationToken= default);

    }
}
