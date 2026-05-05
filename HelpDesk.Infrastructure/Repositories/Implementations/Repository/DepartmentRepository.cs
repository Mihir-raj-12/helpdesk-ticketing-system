using HelpDesk.Core.DTOs.Dashboard;
using HelpDesk.Core.Entities;
using HelpDesk.Core.Enums;
using HelpDesk.Core.Interfaces;
using HelpDesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Infrastructure.Repositories.Implementations.Repository
{
    public class DepartmentRepository : GenericRepository<Department>, IDepartmentRepository
    {
        public DepartmentRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Department>> GetAllWithDetailsAsync()
        {
            return await _dbSet
                .Include(d => d.DepartmentHead)
                .Include(d => d.Users)
                .Where(d => d.IsActive)
                .ToListAsync();
        }

        public async Task<Department?> GetByIdWithUsersAsync(int id)
        {
            return await _dbSet
                .Include(d => d.Users)
                .FirstOrDefaultAsync(d => d.Id == id && d.IsActive);
        }

        

    }
}
