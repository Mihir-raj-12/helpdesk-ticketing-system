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

            // 1. Write the CSV Header row (PRD 8.3 required fields)
            sb.AppendLine("Timestamp (UTC),Actor Name,Action Type,Entity Type,Entity ID,IP Address,Changes");

            // 2. Loop through the logs and write the data rows
            foreach (var log in logs)
            {
                var actorName = log.PerformedByUser != null ? log.PerformedByUser.FullName : "System";

                // Flatten the Audit Details into a single readable string for the CSV cell
                var changes = string.Join(" | ", log.AuditDetails.Select(d => $"{d.FieldName}: '{d.OldValue}' -> '{d.NewValue}'"));

                // Use quotes around fields that might contain commas (like our changes string)
                sb.AppendLine($"{log.PerformedAt:yyyy-MM-dd HH:mm:ss},{actorName},{log.Action},{log.TableName},{log.EntityId},{log.IpAddress},\"{changes}\"");
            }

            // 3. Convert the string into a UTF-8 byte array so it can be downloaded
            return Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}
