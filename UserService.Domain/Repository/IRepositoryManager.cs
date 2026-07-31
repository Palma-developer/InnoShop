using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.Domain.Repository
{
    public class IRepositoryManager
    {
        public IUserRepository UserRepository { get; set; }
        public IUnitOfWork UnitOfWork { get; set; }
    }
}
