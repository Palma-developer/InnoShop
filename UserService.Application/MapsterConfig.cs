using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Domain.ValueObject;
using UserService.Domain.Entities;
using UserService.Service.Abstraction.Models;

namespace UserService.Service
{
    public static class MapsterConfig
    {
        public static void Register()
        {
            TypeAdapterConfig<User, UserDTO>
                .NewConfig()
                .Map(dest => dest.Email, src => src.Email.Value);

            TypeAdapterConfig<UserDTO, User>
                .NewConfig()
                .Map(dest => dest.Email, src => new Email(src.Email));
        }
    }
}
