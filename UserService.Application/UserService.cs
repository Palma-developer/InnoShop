using System.Runtime.CompilerServices;
using UserService.Domain.Repository;
using UserService.Service.Abstraction;
using UserService.Service.Abstraction.Models;
using Mapster;
using UserService.Domain.Entities;
using UserService.Domain.Enums;

namespace UserService.Service
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

        public async Task<IEnumerable<UserDTO>> GetAllActiveAsync(CancellationToken cancellationToken = default)
        {
            var users = await _repositoryManager.UserRepository.GetAllActiveAsync(cancellationToken);


            var usersDto = users.Adapt<IEnumerable<UserDTO>>();

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
        public async Task<UserDTO> CreateAsync(UserDTO userDTO, CancellationToken cancellationToken = default)
        {
            var user=userDTO.Adapt<User>();
            _repositoryManager.UserRepository.Insert(user);
            await _repositoryManager.UnitOfWork.SaveChangesAsync(cancellationToken);
            return user.Adapt<UserDTO>();
        }

        public async Task UpdateAsync(int id, UserDTO userDTO, CancellationToken cancellationToken = default)
        {
            var user = await _repositoryManager.UserRepository.GetByIdAsync(id, cancellationToken);

            if (user is null)
            {
                throw new ArgumentException(Convert.ToString(id));
            }

            
            if (!Enum.TryParse<UserRole>(userDTO.Role, out var role))
            {
                throw new ArgumentException("Invalid role");
            }

            user.Name = userDTO.Name;
            user.Email.Value = userDTO.Email;
            user.EmailConfirmed = userDTO.EmailConfirmed;
            user.IsActive = userDTO.IsActive;
            user.Role = role;
            user.Password = userDTO.Password;

            _repositoryManager.UserRepository.Update(user);

            await _repositoryManager.UnitOfWork.SaveChangesAsync(cancellationToken);


        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var user= await _repositoryManager.UserRepository.GetByIdAsync(id, cancellationToken);

            if (user is null)
            {
                throw new ArgumentException(Convert.ToString(id));
            }
            user.IsActive=false;
            _repositoryManager.UserRepository.Delete(user);

            await _repositoryManager.UnitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
