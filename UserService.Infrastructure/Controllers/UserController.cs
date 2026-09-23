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
        
    }
}