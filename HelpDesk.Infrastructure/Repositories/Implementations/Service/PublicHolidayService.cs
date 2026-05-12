using AutoMapper;
using HelpDesk.Core.DTOs.Holidays;
using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Infrastructure.Repositories.Implementations.Service
{
    public class PublicHolidayService : GenericService<PublicHolidayResponseDto, PublicHoliday>, IPublicHolidayService
    {
        public PublicHolidayService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserProvider currentUserProvider)
            : base(mapper, unitOfWork, currentUserProvider, unitOfWork.PublicHolidays)
        {
        }
    }
}
