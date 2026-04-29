using AutoMapper;
using HelpDesk.Core.Common;
using HelpDesk.Core.DTOs.Settings;
using HelpDesk.Core.Interfaces;
using HelpDesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Infrastructure.Repositories.Implementations.Service
{


    public class SystemSettingService : ISystemSettingService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SystemSettingService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse<SystemSettingDto>> GetSettingsAsync()
        {
            // Using GenericRepository's GetByIdAsync to fetch the single row (Id = 1)
            var settings = await _unitOfWork.SystemSettings.GetByIdAsync(1);
            if (settings == null) return ApiResponse<SystemSettingDto>.Failure("Settings not found.");

            var dto = _mapper.Map<SystemSettingDto>(settings);
            return ApiResponse<SystemSettingDto>.Success(dto);
        }

        public async Task<ApiResponse<SystemSettingDto>> UpdateSettingsAsync(SystemSettingDto dto)
        {
            var settings = await _unitOfWork.SystemSettings.GetByIdAsync(1);
            if (settings == null) return ApiResponse<SystemSettingDto>.Failure("Settings not found.");

            // Map incoming DTO over the existing entity
            _mapper.Map(dto, settings);

            // Using your GenericRepository UpdateAsync!
            await _unitOfWork.SystemSettings.UpdateAsync(settings);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<SystemSettingDto>.Success(dto, "Settings updated successfully.");
        }
    }
}
