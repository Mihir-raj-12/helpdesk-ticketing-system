using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using HelpDesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Infrastructure.Repositories.Implementations
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
          return await _dbSet.FirstOrDefaultAsync(e => e.Id == id && e.isActive);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
           return await _dbSet.Where(e => e.isActive).ToListAsync();
        }

        public async Task<T> AddAsync(T entity)
        {
            entity.CreatedDate = DateTime.UtcNow;
            entity.LastUpdatedDate = DateTime.UtcNow;
            entity.isActive = true;
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public async Task UpdateAsync(T entity)
        {
           entity.LastUpdatedDate = DateTime.UtcNow;
            _dbSet.Update(entity);
        }

        public async Task SoftDeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if(entity != null)
            {
                entity.isActive = false;
                entity.LastUpdatedDate = DateTime.UtcNow;

            }
        }


    }
}
