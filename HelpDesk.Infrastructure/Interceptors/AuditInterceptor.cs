using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Infrastructure.Interceptors
{
    public class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly ICurrentUserProvider _currentUserProvider;

        public AuditInterceptor(ICurrentUserProvider currentUserProvider)
        {
            _currentUserProvider = currentUserProvider;
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context == null) return base.SavingChangesAsync(eventData, result, cancellationToken);

            var userId = _currentUserProvider.GetCurrentUserId() ?? "System";
            var auditEntries = new List<AuditLog>();

            // Look at everything EF Core is about to save
            var entries = context.ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity &&
                            e.State != EntityState.Detached &&
                            e.State != EntityState.Unchanged)
                .ToList();

            foreach (var entry in entries)
            {
                // We do NOT want to audit the Audit tables themselves (Infinite loop!)
                if (entry.Entity is AuditLog || entry.Entity is AuditDetail)
                    continue;

                var tableName = entry.Entity.GetType().Name;
                var action = entry.State.ToString(); // "Added", "Modified", or "Deleted"

                var auditLog = new AuditLog
                {
                    TableName = tableName,
                    Action = action,
                    PerformedByUserId = userId,
                    PerformedAt = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow,
                    LastUpdatedDate = DateTime.UtcNow,
                    IsActive = true
                };

                // Track the specific fields that changed
                foreach (var property in entry.Properties)
                {
                    // Skip tracking EF Core's hidden temporary properties
                    if (property.IsTemporary) continue;

                    string propertyName = property.Metadata.Name;

                    if (entry.State == EntityState.Added)
                    {
                        auditLog.AuditDetails.Add(new AuditDetail
                        {
                            FieldName = propertyName,
                            NewValue = property.CurrentValue?.ToString(),
                            CreatedDate = DateTime.UtcNow,
                            LastUpdatedDate = DateTime.UtcNow,
                            IsActive = true
                        });
                    }
                    else if (entry.State == EntityState.Modified && property.IsModified)
                    {
                        var originalValue = property.OriginalValue?.ToString();
                        var currentValue = property.CurrentValue?.ToString();

                        // Only log it if the value actually changed!
                        if (originalValue != currentValue)
                        {
                            auditLog.AuditDetails.Add(new AuditDetail
                            {
                                FieldName = propertyName,
                                OldValue = originalValue,
                                NewValue = currentValue,
                                CreatedDate = DateTime.UtcNow,
                                LastUpdatedDate = DateTime.UtcNow,
                                IsActive = true
                            });
                        }
                    }
                }

                // If we recorded any field changes, add the log to our list
                if (auditLog.AuditDetails.Any())
                {
                    auditEntries.Add(auditLog);
                }
            }

            // Inject the new audit logs into the current save operation
            if (auditEntries.Any())
            {
                context.AddRange(auditEntries);
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

    }
}
