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

            var userId = _currentUserProvider.GetCurrentUserId();
            var ipAddress = _currentUserProvider.GetClientIpAddress() ?? "System/Unknown";
            var userRole = _currentUserProvider.GetCurrentUserRole() ?? "System";
            // --- FIX 1 (L28): Get actual snapshot details ---
            string actorName = "System";
            string actorEmail = "system@system.com";

            if (!string.IsNullOrEmpty(userId))
            {
                // Check local cache first, then database
                var user = context.Set<ApplicationUser>().Local.FirstOrDefault(u => u.Id == userId)
                           ?? context.Set<ApplicationUser>().FirstOrDefault(u => u.Id == userId);

                if (user != null)
                {
                    actorName = user.FullName;
                    actorEmail = user.Email ?? "Unknown";
                }
            }

            var auditEntries = new List<AuditLog>();

            var entries = context.ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity &&
                            e.State != EntityState.Detached &&
                            e.State != EntityState.Unchanged)
                .ToList();

            foreach (var entry in entries)
            {
                // --- STEP 1: THE SECURITY BLOCK (PRD 8.3) ---
                if (entry.Entity is AuditLog || entry.Entity is AuditDetail)
                {
                    // If someone tries to edit or delete an audit record, crash the transaction!
                    if (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                    {
                        throw new InvalidOperationException("SECURITY EXCEPTION: Audit records are strictly append-only. Modifying or deleting them is a violation of PRD 8.3.");
                    }

                    // If it's just being Added (EntityState.Added), let it pass through.
                    continue;
                }

                var tableName = entry.Entity.GetType().Name;
                var action = entry.State.ToString(); // "Added", "Modified", or "Deleted"
                var primaryKeyProperty = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
                var entityId = primaryKeyProperty?.CurrentValue?.ToString() ?? "Unknown";

                var auditLog = new AuditLog
                {
                    TableName = tableName,
                    EntityId = entityId,    // NEW
                    IpAddress = ipAddress,  // NEW
                    Action = action,
                    PerformedByUserId = userId,
                    // --- NEW SNAPSHOT FIELDS ---
                    ActorName = actorName,
                    ActorEmail = actorEmail,
                    ActorRole = userRole,
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
