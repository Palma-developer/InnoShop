using System.Runtime.CompilerServices;
using UserService.Domain.Repository;
using UserService.Service.Abstraction;
using UserService.Service.Abstraction.Models;
using Mapster;
using UserService.Domain.Entities;
using UserService.Domain.Enums;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using MailKit.Net.Smtp;

namespace UserService.Service
{
    internal sealed class UserService : IUserService
    {
        private readonly IRepositoryManager _repositoryManager;

        public UserService(IRepositoryManager repositoryManager) => _repositoryManager = repositoryManager;
        public async Task<IEnumerable<UserDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {

            var users = await _repositoryManager.UserRepository.GetAllAsync(cancellationToken);


            var usersDto = users.Adapt<IEnumerable<UserDTO>>();

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
            var user = await _repositoryManager.UserRepository.GetByIdAsync(id, cancellationToken);
            if (user == null)
            {
                throw new ArgumentException(Convert.ToString(id));
            }
            var userDto = user.Adapt<UserDTO>();
            return userDto;
        }
        //метод регистрации
        public async Task<UserDTO> CreateAsync(UserDTO userDTO, CancellationToken cancellationToken = default)
        {
            var user = userDTO.Adapt<User>();
            _repositoryManager.UserRepository.Insert(user);
            await _repositoryManager.UnitOfWork.SaveChangesAsync(cancellationToken);
            return user.Adapt<UserDTO>();
        }
        //метод логина
        public async Task<string> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            var user = await _repositoryManager.UserRepository.GetByEmail(email);
            if (user == null)
            {
                throw new ArgumentException(email);
            }

            if (user.Password != password)
            {
                throw new ArgumentException("Uncorrect password");
            }
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, user.Name),
                                         new Claim (ClaimTypes.Email, user.Email.Value),
                                         new Claim(ClaimTypes.Role, user.Role.ToString()),
                                         new Claim("EmailConfirm" , user.EmailConfirmed.ToString()),
                                         new Claim("id", user.Id.ToString())};
            var jwt = new JwtSecurityToken(
                issuer: AuthOptions.ISSURE,
                audience: AuthOptions.AUDIENCE,
                claims: claims,
                expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(30)),
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            return new JwtSecurityTokenHandler().WriteToken(jwt);
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
            var user = await _repositoryManager.UserRepository.GetByIdAsync(id, cancellationToken);

            if (user is null)
            {
                throw new ArgumentException(Convert.ToString(id));
            }
            user.IsActive = false;
            _repositoryManager.UserRepository.Delete(user);

            await _repositoryManager.UnitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task SendEmailAsync(string email, string message, CancellationToken cancellationToken = default)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("Администрация InnoShop", "innoshop@gmail.com"));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = "Подтверждение почты";
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = message
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.gmail.com", 465, true);
                await client.AuthenticateAsync("innoshop@gmail.com", "password");//создать почту и вписать пароль
                await client.SendAsync(emailMessage);

                await client.DisconnectAsync(true);
            }
        }

    }
}

