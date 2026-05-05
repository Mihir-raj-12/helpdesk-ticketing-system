using HelpDesk.Core.Common;
using HelpDesk.Core.DTOs.Dashboard;
using HelpDesk.Core.Interfaces;

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
    }
}