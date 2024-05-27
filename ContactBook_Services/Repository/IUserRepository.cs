using ContactBook_Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactBook_Services.Repository
{
    public interface IUserRepository : IRepository<User,string>
    {
        Task<(List<User>,PaginationMetaData)> GetUsersAsyncPagination(int pageNumber, int pageSize);
    }
}
