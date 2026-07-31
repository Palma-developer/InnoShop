using UserService.Domain.Enums;
using UserService.Domain.ValueObject;
namespace UserService.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Email Email { get; set; }
        public UserRole Role { get; set; }
        public string Password { get; set; }
        public string EmailConfirmed { get; set; } = string.Empty;
        public bool IsActive { get; set; }


    }
}
