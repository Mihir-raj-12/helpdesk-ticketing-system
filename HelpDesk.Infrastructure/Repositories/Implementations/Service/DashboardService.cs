using HelpDesk.Core.Common;
using HelpDesk.Core.DTOs.Dashboard;
using HelpDesk.Core.Enums;
using HelpDesk.Core.Interfaces;
using System.Text;

namespace HelpDesk.Infrastructure.Repositories.Implementations.Service
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserProvider _currentUserProvider;

        public DashboardService(IUnitOfWork unitOfWork, ICurrentUserProvider currentUserProvider)
        {
            _unitOfWork = unitOfWork;
            _currentUserProvider = currentUserProvider;
        }

        public async Task<ApiResponse<DashboardResponseDto>> GetDashboardStatusAsync()
        {
            try
            {
                var currentUserId = _currentUserProvider.GetCurrentUserId();
                var currentUserRole = _currentUserProvider.GetCurrentUserRole();

                if (string.IsNullOrEmpty(currentUserId))
                    return ApiResponse<DashboardResponseDto>.Failure("User not found.");

                var now = DateTime.UtcNow;
                var startOfThisMonth = new DateTime(now.Year, now.Month, 1);
                var startOfLastMonth = startOfThisMonth.AddMonths(-1);

                // Send the request down to the high-performance SQL query!
                var stats = await _unitOfWork.Tickets.GetDashboardMetricsAsync(
                    startOfThisMonth,
                    startOfLastMonth,
                    currentUserId,
                    currentUserRole);

                return ApiResponse<DashboardResponseDto>.Success(stats);
            }
            catch (Exception ex)
            {
                return ApiResponse<DashboardResponseDto>.Failure($"Failed to load dashboard data: {ex.Message}");
            }
        }

        public async Task<(byte[] FileContents, string ContentType, string FileName)> ExportActionableTicketsCsvAsync()
        {
            var currentUserId = _currentUserProvider.GetCurrentUserId();
            var currentUserRole = _currentUserProvider.GetCurrentUserRole();

            // 1. Get the data
            var tickets = await _unitOfWork.Tickets.GetActionableTicketsForExportAsync(currentUserId, currentUserRole);

            // 2. Build the CSV (PRD 7.5: Must include header row)
            var builder = new StringBuilder();
            builder.AppendLine("Ticket ID,Title,Priority,Status,Raised By,Assigned To,Created Date,SLA Deadline,Is Escalated,Is Breached");

            var now = DateTime.UtcNow;

            // 3. Loop through and create rows
            foreach (var t in tickets)
            {
                var isBreached = (t.SlaDeadline <= now && t.Status != TicketStatus.Resolved && t.Status != TicketStatus.Closed);

                // CSVs break if a user puts a comma in the ticket title! We replace commas with spaces to be safe.
                var safeTitle = t.Title?.Replace(",", " ") ?? "No Title";
                var raisedBy = t.RaisedByUser?.FullName ?? "Unknown";
                var assignedTo = t.AssignedToUser?.FullName ?? "Unassigned";

                builder.AppendLine($"{t.Id},{safeTitle},{t.Priority},{t.Status},{raisedBy},{assignedTo},{t.CreatedDate:yyyy-MM-dd HH:mm},{t.SlaDeadline:yyyy-MM-dd HH:mm},{t.EscalationFlag},{isBreached}");
            }

            // 4. The Magic Trick: Add UTF-8 BOM so Excel instantly formats it into columns
            var fileContents = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(builder.ToString())).ToArray();

            // 5. Return the file package
            return (fileContents, "text/csv", $"UrgentTickets_Report_{DateTime.UtcNow:yyyyMMdd}.csv");
        }
    }
}