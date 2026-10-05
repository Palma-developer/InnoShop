using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace ProductService.Service
{

    public class AuthOptions
    {
        public const string ISSURE = "UserService";
        public const string AUDIENCE = "UserClient";
        const string KEY = "InnoShop_UserService_SuperStrongKey_2026_Secret_Key_For_JWT_Signing";

        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }
    }

}
