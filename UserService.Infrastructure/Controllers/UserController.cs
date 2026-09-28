using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Service.Abstraction;
using UserService.Service.Abstraction.Models;
 

namespace  UserService.Infrastructure.Presintation.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserControler : ControllerBase
    {
        private readonly IServiceManager _serviceManager;

        public UserControler(IServiceManager serviceManager) => _serviceManager = serviceManager;

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
        {
            var users = await _serviceManager.UserService.GetAllAsync(cancellationToken);

            return Ok(users);
        }
        [Authorize]
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveUsers(CancellationToken cancellationToken)
        {
            var users = await _serviceManager.UserService.GetAllActiveAsync(cancellationToken);

            return Ok(users);
        }

        
        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(int userId, string code)
        {
            var result=await _serviceManager.UserService.ConfirmEmailAsync(userId, code);
            return Ok(result);
        }


        [Authorize]
        [HttpGet("{userId:int}")]
        public async Task<IActionResult> GetUserById(int userId, CancellationToken cancellationToken)
        {
            var userDto = await _serviceManager.UserService.GetByIdAsync(userId, cancellationToken);

            return Ok(userDto);
        }
        //регистрация
        [HttpPost("registration")]
        public async Task<IActionResult> CreateUser([FromBody] UserDTO userDtoForCreate)
        {
            var userDto = await _serviceManager.UserService.CreateAsync(userDtoForCreate);

            var code= Guid.NewGuid().ToString();
            await _serviceManager.UserService.SaveConfirmationCodeAsync(userDto.Id, code);

            /*var callbackUrl = Url.Action(
                "ConfirmEmail",
                "User",
                new { userId = userDto.Id, code = code },
                protocol: HttpContext.Request.Scheme);
            */
            var encodedCode = Uri.EscapeDataString(code);
            var callbackUrl = $"http://localhost:5111/api/users/ConfirmEmail?userId={userDto.Id}&code={encodedCode}";
            Console.WriteLine($"[ОТЛАДКА] Значение callbackUrl: '{callbackUrl}'");
            Console.WriteLine($"[ОТЛАДКА] Длина строки: {callbackUrl?.Length ?? 0}");
            //var htmlMessage = $"Подтвердите регистрацию, перейдя по ссылке: <a href=\"https://google.com\" style=\"color: blue; text-decoration: underline;\">Подтвердить email (Тест Google)</a>";
            var htmlMessage = $"Подтвердите регистрацию, перейдя по ссылке: <a href=\"{callbackUrl}\">Подтвердить email</a>";
            await _serviceManager.UserService.SendEmailAsync(userDto.Email, htmlMessage);


            return CreatedAtAction(nameof(GetUserById), new { userId = userDto.Id }, userDto);
        }
        

        [HttpPost("login")]
        public async Task<IActionResult> Login(string email, string password, CancellationToken cancellationToken)
        {
            var token=await _serviceManager.UserService.LoginAsync(email, password, cancellationToken);
            return Ok(token);
        }

        [Authorize]
        [HttpPut("{userId:int}")]
        public async Task<IActionResult> UpdateUser(int userId, [FromBody] UserDTO userDtoForUpdate, CancellationToken cancellationToken)
        {
            await _serviceManager.UserService.UpdateAsync(userId, userDtoForUpdate, cancellationToken);
            return NoContent();
        }

        [Authorize]
        [HttpDelete("{userId:int}")]
        public async Task<IActionResult> DeleteUser(int userId, CancellationToken cancellationToken)
        {
            await _serviceManager.UserService.DeleteAsync(userId, cancellationToken);

            return NoContent();
        }

        [HttpPost ("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
        {
            await _serviceManager.UserService.RequestPasswordResetAsync(request.Email, cancellationToken);
            return Ok("Если пользователь с таким email существует, дальнешие дейсвтвия отправлены на почту");
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody]ResetPasswordRequest request, CancellationToken cancellationToken)
        {
            try
            {
                await _serviceManager.UserService.ResetPasswordAsync(request.UserId, request.Token, request.NewPassword, cancellationToken);
                return Ok("Пароль успешно изменен");

            }
            catch(InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet ("reset-password")]
        public async Task<IActionResult> ValidateResetToken([FromQuery] int userId,[FromQuery] string token, CancellationToken cancellationToken)
        {
            try
            {
                await _serviceManager.UserService.ValidateResetTokenAsync(userId, token, cancellationToken);
                return Ok($" Токен действителен!\n\nДля завершения сброса пароля откройте Swagger и отправьте POST-запрос на /api/users/reset-password со следующими данными:\n\n{{\n  \"userId\": {userId},\n  \"token\": \"{token}\",\n  \"newPassword\": \"ВашНовыйПароль123\"\n}}");
            }
            catch (InvalidOperationException ex)
            {
                
                return BadRequest(ex.Message);
            }
        }

    }
}