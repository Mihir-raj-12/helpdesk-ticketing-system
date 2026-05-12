using AutoMapper;
using HelpDesk.Core.Common;
using HelpDesk.Core.DTOs.Audit;
using HelpDesk.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Infrastructure.Repositories.Implementations.Service
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AuditLogService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse<IEnumerable<AuditLogResponseDto>>> GetRecentLogsAsync()
        {
            // Fetch top 100 recent logs using the custom repository
            var logs = await _unitOfWork.AuditLogs.GetRecentAuditLogsAsync(100);

            // Map to the safe, flattened DTOs
            var responseDto = _mapper.Map<IEnumerable<AuditLogResponseDto>>(logs);

            return ApiResponse<IEnumerable<AuditLogResponseDto>>.Success(responseDto);
        }

        public async Task<byte[]> ExportLogsToCsvAsync(DateTime startDate, DateTime endDate)
        {
            var logs = await _unitOfWork.AuditLogs.GetLogsByDateRangeAsync(startDate, endDate);

            var sb = new StringBuilder();

            // Headers
            sb.AppendLine("Timestamp (UTC),Actor Name,Actor Email,Actor Role,Action Type,Entity Type,Entity ID,IP Address,Changes");

            foreach (var log in logs)
            {
                var changes = string.Join(" | ", log.AuditDetails.Select(d => $"{d.FieldName}: '{d.OldValue}' -> '{d.NewValue}'"));

                // --- THE FIX: Fallbacks for older test data! ---
                // If ActorName is empty (old data), fall back to the linked User's FullName. Otherwise, use "System".
                var actorName = !string.IsNullOrWhiteSpace(log.ActorName) ? log.ActorName : (log.PerformedByUser?.FullName ?? "System");
                var actorEmail = !string.IsNullOrWhiteSpace(log.ActorEmail) ? log.ActorEmail : (log.PerformedByUser?.Email ?? "System");
                var actorRole = !string.IsNullOrWhiteSpace(log.ActorRole) ? log.ActorRole : "System";

                // Write the row using our safe fallback variables
                sb.AppendLine($"{log.PerformedAt:yyyy-MM-dd HH:mm:ss},{actorName},{actorEmail},{actorRole},{log.Action},{log.TableName},{log.EntityId},{log.IpAddress},\"{changes}\"");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}
