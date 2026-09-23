using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Domain.Repository;
using UserService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace UserService.Infrastructure.Persistence.Repository
{
    internal sealed class UserRepository:IUserRepository
    {
        private readonly RepositoryDbContext _dbContext;

        public UserRepository(RepositoryDbContext dbContext) => _dbContext = dbContext;

        public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken= default)=>
            await _dbContext.Users.ToListAsync(cancellationToken);

        public async Task<IEnumerable<User>> GetAllActiveAsync(CancellationToken cancellationToken= default)=>
            await _dbContext.Users.Where(u => u.IsActive).ToListAsync(cancellationToken);

        public async Task<User> GetByIdAsync(int id, CancellationToken cancellationToken=default)=>
            await _dbContext.Users.Where(u => u.IsActive == true).FirstOrDefaultAsync(x=>x.Id == id, cancellationToken);

        //получение пользователя по email
        public async Task<User> GetByEmail(string email, CancellationToken cancellationToken = default)=>
            await _dbContext.Users.Where(u=>u.IsActive==true).FirstOrDefaultAsync(x=>x.Email.Value==email, cancellationToken);
        public void Insert(User user)=>_dbContext.Add(user);

        public void Update(User user)=>_dbContext.Update(user);
        public void Delete(User user)=>_dbContext.Update(user);

        
    }
}
