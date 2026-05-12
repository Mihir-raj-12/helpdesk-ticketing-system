using AutoMapper;
using HelpDesk.Core.Common;
using HelpDesk.Core.DTOs.Notifications;
using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Infrastructure.Repositories.Implementations.Service
{
    public class NotificationPreferenceService : INotificationPreferenceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserProvider _currentUserProvider;

        public NotificationPreferenceService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserProvider currentUserProvider)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserProvider = currentUserProvider;
        }
        public async Task<ApiResponse<NotificationPreferenceDto>> GetMyPreferencesAsync()
        {
            var userId = _currentUserProvider.GetCurrentUserId();
            if (string.IsNullOrEmpty(userId)) return ApiResponse<NotificationPreferenceDto>.Failure("User not found.");

            var preferences = (await _unitOfWork.NotificationPreferences.FindAsync(p => p.UserId == userId)).FirstOrDefault();

            if (preferences == null)
            {
                preferences = new NotificationPreference
                {
                    UserId = userId,
                    NotifyOnTicketCreated = true,
                    NotifyOnTicketAssigned = true,
                    NotifyOnStatusChanged = true,
                    NotifyOnNewComment = true,
                    NotifyOnTicketClosed = true,
                    OptOutCsatSurveys = false,
                    CreatedDate = DateTime.UtcNow,
                    LastUpdatedDate = DateTime.UtcNow,
                    IsActive = true
                };

                await _unitOfWork.NotificationPreferences.AddAsync(preferences);
                await _unitOfWork.SaveChangesAsync();
            }

            var dto = _mapper.Map<NotificationPreferenceDto>(preferences);
            return ApiResponse<NotificationPreferenceDto>.Success(dto);
        }
        public async Task<ApiResponse<NotificationPreferenceDto>> UpdateMyPreferencesAsync(NotificationPreferenceDto dto)
        {
            var userId = _currentUserProvider.GetCurrentUserId();
            if (string.IsNullOrEmpty(userId)) return ApiResponse<NotificationPreferenceDto>.Failure("User not found.");

            var preferences = (await _unitOfWork.NotificationPreferences.FindAsync(p => p.UserId == userId)).FirstOrDefault();

            if (preferences == null)
                return ApiResponse<NotificationPreferenceDto>.Failure("Preferences not found.");

            // Sync all the specific toggles!
            preferences.NotifyOnTicketCreated = dto.NotifyOnTicketCreated;
            preferences.NotifyOnTicketAssigned = dto.NotifyOnTicketAssigned;
            preferences.NotifyOnStatusChanged = dto.NotifyOnStatusChanged;
            preferences.NotifyOnNewComment = dto.NotifyOnNewComment;
            preferences.NotifyOnTicketClosed = dto.NotifyOnTicketClosed;
            preferences.OptOutCsatSurveys = dto.OptOutCsatSurveys;
            preferences.LastUpdatedDate = DateTime.UtcNow;

            await _unitOfWork.NotificationPreferences.UpdateAsync(preferences);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<NotificationPreferenceDto>.Success(_mapper.Map<NotificationPreferenceDto>(preferences), "Preferences updated successfully.");
        }
    }
}
