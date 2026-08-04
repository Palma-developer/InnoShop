using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Domain.Repository;

namespace UserService.Infrastructure.Persistence.Repository
{
    public sealed class RepositoryManager:IRepositoryManager
    {
        private readonly RepositoryDbContext _context;

        public RepositoryManager(RepositoryDbContext context)
        {
            _context = context;
        }

        public IUserRepository UserRepository => new UserRepository(_context);

        public IUnitOfWork UnitOfWork => new UnitOfWork(_context);
    }
}
