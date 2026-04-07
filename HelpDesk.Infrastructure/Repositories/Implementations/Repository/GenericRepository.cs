using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using HelpDesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Infrastructure.Repositories.Implementations.Repository
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
          return await _dbSet.FirstOrDefaultAsync(e => e.Id == id && e.IsActive);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
           return await _dbSet.Where(e => e.IsActive).ToListAsync();
        }

        public async Task<T> AddAsync(T entity)
        {
            entity.CreatedDate = DateTime.UtcNow;
            entity.LastUpdatedDate = DateTime.UtcNow;
            entity.IsActive = true;
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public async Task UpdateAsync(T entity, params Expression<Func<T, object>>[] properties)
        {
            entity.LastUpdatedDate = DateTime.UtcNow;

            if (properties.Any())
            {
                _dbSet.Attach(entity);
                var entry = _context.Entry(entity);

               
                entry.Property(x => x.LastUpdatedDate).IsModified = true;
                foreach (var selector in properties)
                {
                    entry.Property(selector).IsModified = true;
                }
            }
            else
            {
               
                _dbSet.Update(entity);
            }

            await Task.CompletedTask; 
        }

        public async Task SoftDeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if(entity != null)
            {
                entity.IsActive = false;
                entity.LastUpdatedDate = DateTime.UtcNow;

            }
        }


    }
}
