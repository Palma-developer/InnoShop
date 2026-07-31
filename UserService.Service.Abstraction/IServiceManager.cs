using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.Service.Abstraction
{
    public interface IServiceManager
    {
        IUserService userService { get; }
    }
}
