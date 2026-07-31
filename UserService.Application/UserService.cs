using System.Runtime.CompilerServices;
using UserService.Domain.Repository;
using UserService.Service.Abstraction;
using UserService.Service.Abstraction.Models;
using Mapster;

namespace UserService.Application
{
    internal sealed class UserService:IUserService
    {
        private readonly IRepositoryManager _repositoryManager;

        public UserService(IRepositoryManager repositoryManager)=>_repositoryManager = repositoryManager;
        public async Task<IEnumerable<UserDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            
            var users = await _repositoryManager.UserRepository.GetAllAsync(cancellationToken);

            var usersDto=users.Adapt<IEnumerable<UserDTO>>();

            return usersDto;

        }

        public async Task<UserDTO> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var user= await _repositoryManager.UserRepository.GetByIdAsync(id, cancellationToken);
            if (user == null)
            {
                throw new ArgumentException(Convert.ToString(id));
            }
            var userDto=user.Adapt<UserDTO>();
            return userDto;
        }
        //public async Task<UserDTO>  
    }
}
