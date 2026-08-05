using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Domain.Entities;

namespace UserService.Domain.Repository
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default);

        Task <User> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        void Insert(User user);

        void Update(User user);
        void Delete(User user);

    }
}
