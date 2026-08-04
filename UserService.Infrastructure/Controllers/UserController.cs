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

        [HttpGet]
        public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
        {
            var users = await _serviceManager.UserService.GetAllAsync(cancellationToken);

            return Ok(users);
        }

        [HttpGet("{userId:int}")]
        public async Task<IActionResult> GetUserById(int userId, CancellationToken cancellationToken)
        {
            var userDto = await _serviceManager.UserService.GetByIdAsync(userId, cancellationToken);

            return Ok(userDto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserDTO userDtoForCreate)
        {
            var userDto = await _serviceManager.UserService.CreateAsync(userDtoForCreate);

            return CreatedAtAction(nameof(GetUserById), new { userId = userDto.Id }, userDto);
        }

        [HttpPut("{userId:int}")]
        public async Task<IActionResult> UpdateUser(int userId, [FromBody] UserDTO userDtoForUpdate, CancellationToken cancellationToken)
        {
            await _serviceManager.UserService.UpdateAsync(userId, userDtoForUpdate, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{userId:int}")]
        public async Task<IActionResult> DeleteUser(int userId, CancellationToken cancellationToken)
        {
            await _serviceManager.UserService.DeleteAsync(userId, cancellationToken);

            return NoContent();
        }
    }
}