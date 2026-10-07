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
using System.Diagnostics.Contracts;
using Microsoft.Extensions.Configuration;

namespace UserService.Service
{
    public sealed class UserService : IUserService
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly IConfiguration _configuration;
        public UserService(IRepositoryManager repositoryManager, IConfiguration configuration)
        {
            _repositoryManager = repositoryManager;
            _configuration = configuration;
        }

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
                throw new ArgumentException("Пользователь не найден, id: "+ Convert.ToString(id));
            }
            var userDto = user.Adapt<UserDTO>();
            return userDto;
        }
        //метод регистрации
        public async Task<UserDTO> CreateAsync(UserForCreate userForCreateDTO, CancellationToken cancellationToken = default)
        {

            if (!EmailCorect.IsValidEmail(userForCreateDTO.Email))
            {
                throw new InvalidOperationException("Email имеет не верный формат");
            }
            

            if( await _repositoryManager.UserRepository.GetByEmail(userForCreateDTO.Email, cancellationToken) != null)
            {
                throw new InvalidOperationException("Пользовател с такой почтой уже зарегистрирован");
            }
            var userDto = new UserDTO();
            
            userDto.IsActive=true;
            userDto.Email=userForCreateDTO.Email;
            userDto.Name=userForCreateDTO.Name;
            userDto.Password=userForCreateDTO.Password;
            userDto.Role=userForCreateDTO.Role;
            userDto.EmailConfirmed = false;

            var user = userDto.Adapt<User>();
            _repositoryManager.UserRepository.Insert(user);
            await _repositoryManager.UnitOfWork.SaveChangesAsync(cancellationToken);
            return user.Adapt<UserDTO>();
        }
        //метод логина
        public async Task<string> LoginAsync(UserForLogin userForLogin, CancellationToken cancellationToken = default)
        {

            if (!EmailCorect.IsValidEmail(userForLogin.Email))
            {
                throw new InvalidOperationException("Email имеет не верный формат");
            }

            var user = await _repositoryManager.UserRepository.GetByEmail(userForLogin.Email);
            if (user == null)
            {
                throw new ArgumentException(userForLogin.Email);
            }

            if (user.Password != userForLogin.Password)
            {
                throw new ArgumentException("Uncorrect password");
            }
            if (!user.EmailConfirmed)
            {
                throw new InvalidOperationException("Пользователь не подтвердил свою почту");
            }
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, user.Name),
                                         new Claim (ClaimTypes.Email, user.Email.Value),
                                         new Claim(ClaimTypes.Role, user.Role.ToString()),
                                         new Claim("EmailConfirm" , user.EmailConfirmed.ToString()),
                                         new Claim("id", user.Id.ToString())};
            var jwt = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                claims: claims,
                expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(30)),
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        public async Task UpdateAsync(int id, UserForUpdate userForUpdate, CancellationToken cancellationToken = default)
        {
            var user = await _repositoryManager.UserRepository.GetByIdAsync(id, cancellationToken);

            if (user is null)
            {
                throw new ArgumentException(Convert.ToString(id));
            }


            if (!Enum.TryParse<UserRole>(userForUpdate.Role, out var role))
            {
                throw new ArgumentException("Invalid role");
            }

            user.Name = userForUpdate.Name;
            user.Role = role;
            

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
            using var httpClient= new HttpClient();
            var productServiceUrl = _configuration["ProductServiceBaseUrl"] ?? "http://localhost:5077";
            await httpClient.PutAsync($"{productServiceUrl}/api/products/hide-by-user/{id}", null);
           
            _repositoryManager.UserRepository.Delete(user);

            await _repositoryManager.UnitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task SendEmailAsync(string email, string message, CancellationToken cancellationToken = default)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("Администрация InnoShop", "innoshop7227@gmail.com"));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = "Подтверждение почты";
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = message
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.gmail.com", 465, true);
                await client.AuthenticateAsync("innoshop7227@gmail.com", "ruioqwhorlzfdwsu");//создать почту и вписать пароль
                await client.SendAsync(emailMessage);

                await client.DisconnectAsync(true);
            }
        }
        public string GenerationEmailConfirmationToken()
        {
            return Guid.NewGuid().ToString();
        }

        public async Task SaveConfirmationCodeAsync(int id, string code, CancellationToken cancellationToken = default)
        {
            var user= await _repositoryManager.UserRepository.GetByIdAsync(id);

            if (user is null)
            {
                throw new Exception($"Пользователь {id} не найден");
            }
            user.EmailConfirmationToken = code;
            await _repositoryManager.UnitOfWork.SaveChangesAsync(cancellationToken);
        }
        public async Task<bool> ConfirmEmailAsync(int id, string code, CancellationToken cancellationToken=default)
        {
            var user = await _repositoryManager.UserRepository.GetByIdAsync(id);

            if(user is null)
            {
                return false;
            }
            if (user.EmailConfirmationToken != code)
            {
                return false;
            }
            user .EmailConfirmed = true;

            await _repositoryManager.UnitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default)
        {

            if (!EmailCorect.IsValidEmail(email))
            {
                throw new InvalidOperationException("Email имеет не верный формат");
            }
            var user = await _repositoryManager.UserRepository.GetByEmail(email);
            if (user is null)
            {
                return;
            }

            var resetToken = Guid.NewGuid().ToString("N");
            user.PasswordResetToken = resetToken;
            user.PasswordResetTokenExprice = DateTime.UtcNow.AddMinutes(15);

            _repositoryManager.UserRepository.Update(user);

            await _repositoryManager.UnitOfWork.SaveChangesAsync(cancellationToken);


            var encodedToken= Uri.EscapeDataString(resetToken);
            var callbackUrl= $"http://localhost:5111/api/users/reset-password?userId={user.Id}&token={encodedToken}";

            var htmlMessage= $"Для сброса пароля перейдите по ссылке (действует 15 минут): <br><br>" +
                      $"<a href=\"{callbackUrl}\" \">Сбросить пароль</a>";
            await SendEmailAsync(user.Email.Value, htmlMessage, cancellationToken);
        }

        public async Task ResetPasswordAsync(int userId, string token, string newPassword, CancellationToken cancellationToken = default)
        {
            var user =await _repositoryManager.UserRepository.GetByIdAsync(userId);
            if (user is null || user.PasswordResetToken != token)
            {
                throw new InvalidOperationException("Неверная или просроченая ссылка для сброса пароля");
            }
            if (user.PasswordResetTokenExprice < DateTime.UtcNow)
            {
                throw new InvalidOperationException("Срок действия ссылки истек");
            }
            user.Password=newPassword;

            user.PasswordResetToken = null;
            user.PasswordResetTokenExprice = null;

            _repositoryManager.UserRepository.Update(user);

            await _repositoryManager.UnitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task ValidateResetTokenAsync(int userId, string token, CancellationToken cancellationToken = default)
        {
            var user = await _repositoryManager.UserRepository.GetByIdAsync(userId);

            if (user == null || user.PasswordResetToken != token)
            {
                throw new InvalidOperationException("Неверная или просроченная ссылка для сброса пароля.");
            }

            if (user.PasswordResetTokenExprice < DateTime.UtcNow)
            {
                throw new InvalidOperationException("Срок действия ссылки для сброса пароля истек.");
            }
        }
        public async Task IsActiveUser(string email, CancellationToken cancellationToken = default)
        {
            if (!EmailCorect.IsValidEmail(email))
            {
                throw new InvalidOperationException("Email имеет не верный формат");
            }

            var user= await _repositoryManager.UserRepository.GetByEmailNoActive(email);
            if (user is null)
            {
                throw new InvalidOperationException("Ползователь с таким email не найден или уже активен");
            }
            user.IsActive = true;
            var httpClient=new HttpClient();
            var productServiceUrl = _configuration["ProductServiceBaseUrl"] ?? "http://localhost:5077";
            await httpClient.PutAsync($"{productServiceUrl}/api/products/show-by-user/{user.Id}", null);
            //await httpClient.PutAsync($"http://localhost:5077/api/products/show-by-user/{user.Id}", null);
            await _repositoryManager.UnitOfWork.SaveChangesAsync(cancellationToken);

        }
    }
}

