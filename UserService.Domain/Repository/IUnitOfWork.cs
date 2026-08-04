using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.Domain.Repository
{
    public interface IUnitOfWork
    {
       public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
