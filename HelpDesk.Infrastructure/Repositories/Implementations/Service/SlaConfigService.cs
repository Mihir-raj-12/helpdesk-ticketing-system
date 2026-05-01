using AutoMapper;
using HelpDesk.Core.Common;
using HelpDesk.Core.DTOs.Settings;
using HelpDesk.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Infrastructure.Repositories.Implementations.Service
{
    public class SlaConfigService : ISlaConfigService

    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SlaConfigService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse<IEnumerable<SlaConfigDto>>> GetAllConfigsAsync()
        {
            var configs = await _unitOfWork.SlaConfigs.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<SlaConfigDto>>(configs);
            return ApiResponse<IEnumerable<SlaConfigDto>>.Success(dtos);
        }

        public async Task<ApiResponse<SlaConfigDto>> UpdateConfigAsync(int id, SlaConfigDto dto)
        {
            if (id != dto.Id)
                return ApiResponse<SlaConfigDto>.Failure("ID mismatch.");

            var existingConfig = await _unitOfWork.SlaConfigs.GetByIdAsync(id);
            if (existingConfig == null)
                return ApiResponse<SlaConfigDto>.Failure("SLA Configuration not found.");

            // Update only the mutable fields (we don't want them changing the Priority Enum!)
            existingConfig.FirstResponseHours = dto.FirstResponseHours;
            existingConfig.ResolutionHours = dto.ResolutionHours;
            existingConfig.WarningThresholdPercent = dto.WarningThresholdPercent;

            await _unitOfWork.SlaConfigs.UpdateAsync(existingConfig);
            await _unitOfWork.SaveChangesAsync();

            var updatedDto = _mapper.Map<SlaConfigDto>(existingConfig);
            return ApiResponse<SlaConfigDto>.Success(updatedDto, "SLA Configuration updated successfully.");
        }

    }

}
