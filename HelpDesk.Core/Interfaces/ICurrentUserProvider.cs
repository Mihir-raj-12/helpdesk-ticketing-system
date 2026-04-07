using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Interfaces
{
    public interface ICurrentUserProvider
    {
        string? GetCurrentUserId();
        string? GetCurrentUserRole();
    }
}
