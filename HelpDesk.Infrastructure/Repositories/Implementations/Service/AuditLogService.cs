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
    }
}
