using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Domain.Repository;

namespace UserService.Infrastructure.Persistence.Repository
{
    public sealed class UnitOfWork:IUnitOfWork
    {
        private readonly RepositoryDbContext _context;

        public UnitOfWork(RepositoryDbContext context) => _context = context;

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }
    }
}
