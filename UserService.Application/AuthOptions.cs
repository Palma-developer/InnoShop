using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace UserService.Service
{
    public class AuthOptions
    {
        public const string ISSUER = "UserService";
        public const string AUDIENCE = "UserClient";
        const string KEY = "InnoShop_UserService_SuperStrongKey_2026_Secret_Key_For_JWT_Signing";
        
        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }
    }
}
