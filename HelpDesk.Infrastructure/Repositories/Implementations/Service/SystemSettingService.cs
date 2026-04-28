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

        private readonly ApplicationDbContext _context;

        public SystemSettingService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<ApiResponse<SystemSettingDto>> GetSettingsAsync()
        {
            var settings = await _context.SystemSettings.FirstOrDefaultAsync(s=>s.Id == 1);
            if(settings  == null)
            {
                return ApiResponse<SystemSettingDto>.Failure("Settings not found");
            }

            var dto = new SystemSettingDto
            {
                SystemName = settings.SystemName,
                SupportEmailAddress = settings.SupportEmailAddress,
                BusinessHourStart = settings.BusinessHourStart,
                BusinessHourEnd = settings.BusinessHourEnd,
                WorkingDays = settings.WorkingDays,
                SlaCriticalResolutionHours = settings.SlaCriticalResolutionHours,
                SlaHighResolutionHours = settings.SlaHighResolutionHours,
                SlaMediumResolutionHours = settings.SlaMediumResolutionHours,
                SlaLowResolutionHours = settings.SlaLowResolutionHours
            };
            return ApiResponse<SystemSettingDto>.Success(dto);

        }

        public async Task<ApiResponse<SystemSettingDto>> UpdateSettingsAsync(SystemSettingDto dto)
        {
            var settings = await _context.SystemSettings.FirstOrDefaultAsync(s => s.Id == 1);

            if (settings == null)
                return ApiResponse<SystemSettingDto>.Failure("System settings not found.");

            // Overwrite the single row
            settings.SystemName = dto.SystemName;
            settings.SupportEmailAddress = dto.SupportEmailAddress;
            settings.BusinessHourStart = dto.BusinessHourStart;
            settings.BusinessHourEnd = dto.BusinessHourEnd;
            settings.WorkingDays = dto.WorkingDays;
            settings.SlaCriticalResolutionHours = dto.SlaCriticalResolutionHours;
            settings.SlaHighResolutionHours = dto.SlaHighResolutionHours;
            settings.SlaMediumResolutionHours = dto.SlaMediumResolutionHours;
            settings.SlaLowResolutionHours = dto.SlaLowResolutionHours;

            await _context.SaveChangesAsync();

            return ApiResponse<SystemSettingDto>.Success(dto, "System settings updated successfully.");
        }
    }
}
