using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Domain.Repository;
using UserService.Service.Abstraction;

namespace UserService.Service
{
    public sealed class ServiceManager:IServiceManager
    {
        private readonly Lazy<IUserService> _lazyUserService;

        public ServiceManager(IRepositoryManager repositoryManager, IConfiguration configuration)
        {
            _lazyUserService = new Lazy<IUserService>(() => new UserService(repositoryManager, configuration));
        }
        public IUserService UserService=>_lazyUserService.Value; 
    }
}
