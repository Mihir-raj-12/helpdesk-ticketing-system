using HelpDesk.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Interfaces
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();

        Task<T> AddAsync(T entity);
   
        Task UpdateAsync(T entity, params Expression<Func<T, object>>[] properties);
        Task SoftDeleteAsync(int id);
    }
}
