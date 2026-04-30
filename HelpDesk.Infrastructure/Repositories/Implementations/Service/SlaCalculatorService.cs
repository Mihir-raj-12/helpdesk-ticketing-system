using HelpDesk.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Infrastructure.Repositories.Implementations.Service
{
    public class SlaCalculatorService : ISlaCalculatorService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SlaCalculatorService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DateTime> CalculateDeadlineAsync(DateTime creationTime, int hoursToAdd)
        {
            // 1. Fetch system configurations
            var settings = await _unitOfWork.SystemSettings.GetByIdAsync(1);
            if (settings == null) return creationTime.AddHours(hoursToAdd); // Fallback

            var holidaysList = await _unitOfWork.PublicHolidays.GetAllAsync();
            var holidayDates = holidaysList.Select(h => h.Date.Date).ToList();

            // Parse the working days string (e.g. "Monday,Tuesday,Wednesday") into an array of Enums
            var workingDays = settings.WorkingDays
                .Split(',')
                .Select(d => Enum.Parse<DayOfWeek>(d.Trim(), true))
                .ToList();

            DateTime currentTime = creationTime;
            double remainingHours = hoursToAdd;

            // Loop until we have successfully added all the required SLA hours
            while (remainingHours > 0)
            {
                // RULE 1: If current day is a weekend OR a public holiday, jump to tomorrow morning
                if (!workingDays.Contains(currentTime.DayOfWeek) || holidayDates.Contains(currentTime.Date))
                {
                    currentTime = currentTime.Date.AddDays(1).Add(settings.BusinessHourStart);
                    continue;
                }

                // RULE 2: If the ticket was created BEFORE business hours (e.g., 6 AM), jump to 9 AM today
                if (currentTime.TimeOfDay < settings.BusinessHourStart)
                {
                    currentTime = currentTime.Date.Add(settings.BusinessHourStart);
                }

                // RULE 3: If we have reached or passed the end of the business day (5 PM), jump to 9 AM tomorrow
                if (currentTime.TimeOfDay >= settings.BusinessHourEnd)
                {
                    currentTime = currentTime.Date.AddDays(1).Add(settings.BusinessHourStart);
                    continue;
                }

                // RULE 4: We are actively inside working hours! Calculate how much time is left today.
                TimeSpan timeTillEndOfDay = settings.BusinessHourEnd - currentTime.TimeOfDay;

                if (remainingHours <= timeTillEndOfDay.TotalHours)
                {
                    // The deadline will be reached today! Add the hours and exit the loop.
                    currentTime = currentTime.AddHours(remainingHours);
                    remainingHours = 0;
                }
                else
                {
                    // The deadline spills over to the next day. 
                    // Subtract today's remaining hours, and jump to tomorrow morning to continue the loop.
                    currentTime = currentTime.Date.AddDays(1).Add(settings.BusinessHourStart);
                    remainingHours -= timeTillEndOfDay.TotalHours;
                }
            }

            return currentTime;
        }
    }
}
