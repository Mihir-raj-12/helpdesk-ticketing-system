using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Interfaces
{
    public interface IAuditRepository
    {
        Task LogAsync(string tableName, string action, string performedByUserId,
            List<(string field, string? oldValue, string? newValue)> changes);
    }
}
