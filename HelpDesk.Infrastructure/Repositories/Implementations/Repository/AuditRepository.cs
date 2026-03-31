using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using HelpDesk.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Infrastructure.Repositories.Implementations.Repository
{
    public class AuditRepository : GenericRepository<AuditLog>, IAuditRepository
    {
        public AuditRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task LogAsync(string tableName, string action,
            string performedByUserId,
            List<(string field, string? oldValue, string? newValue)> changes)
        {
            var auditLog = new AuditLog
            {
                TableName = tableName,
                Action = action,
                PerformedByUserId = performedByUserId,
                PerformedAt = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                isActive = true
            };

            await _dbSet.AddAsync(auditLog);
            await _context.SaveChangesAsync();

            foreach (var change in changes)
            {
                var detail = new AuditDetail
                {
                    AuditLogId = auditLog.Id,
                    FieldName = change.field,
                    OldValue = change.oldValue,
                    NewValue = change.newValue,
                    CreatedDate = DateTime.UtcNow,
                    LastUpdatedDate = DateTime.UtcNow,
                    isActive = true
                };
                _context.AuditDetails.Add(detail);
            }

            await _context.SaveChangesAsync();
        }
    }
}
