using HelpDesk.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelpDesk.Core.Interfaces; 
using HelpDesk.Core.Entities;

namespace HelpDesk.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ITicketRepository Tickets { get; }
        ICommentRepository Comments { get; }
        IAuditLogRepository AuditLogs { get; }
        IGenericRepository<Category> Categories { get; }
        IGenericRepository<AuditDetail> AuditDetails { get; }
        Task<int> SaveChangesAsync();
        IDepartmentRepository Departments { get; }
        IGenericRepository<SystemSetting> SystemSettings { get; }
        IGenericRepository<PublicHoliday> PublicHolidays { get; }
        IGenericRepository<SlaConfig> SlaConfigs { get; }

        IGenericRepository<KbArticle> KbArticles { get; }
        IGenericRepository<KbArticleVersion> KbArticleVersions { get; }

        IGenericRepository<TicketFeedback> TicketFeedbacks { get; }

        IGenericRepository<NotificationPreference> NotificationPreferences { get; }
    }
}
