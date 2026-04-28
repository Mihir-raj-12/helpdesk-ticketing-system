using HelpDesk.Core.Common;
using HelpDesk.Core.Enums;
using HelpDesk.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Infrastructure.Repositories.Implementations.Service
{
    // We are changing the return type to object so we can send an anonymous type
    // containing both the Id and the Name.
    public class LookupService : ILookupService
    {
        public async Task<ApiResponse<List<object>>> GetPrioritiesAsync()
        {
            var priorities = Enum.GetValues(typeof(TicketPriority))
                                 .Cast<TicketPriority>()
                                 .Select(p => new {
                                     id = (int)p,
                                     name = p.ToString()
                                 }).ToList<object>();

            return await Task.FromResult(ApiResponse<List<object>>.Success(priorities));
        }

        public async Task<ApiResponse<List<object>>> GetStatusesAsync()
        {
            var statuses = Enum.GetValues(typeof(TicketStatus))
                               .Cast<TicketStatus>()
                               .Select(s => new {
                                   id = (int)s,
                                   name = s.ToString()
                               }).ToList<object>();

            return await Task.FromResult(ApiResponse<List<object>>.Success(statuses));
        }


        public async Task<ApiResponse<List<object>>> GetRolesAsync()
        {
            var roles = Enum.GetValues(typeof(UserRole))
                            .Cast<UserRole>()
                            .Select(r => new {
                                id = (int)r,
                                name = r.ToString()
                            }).ToList<object>();

            return await Task.FromResult(ApiResponse<List<object>>.Success(roles));
        }
    }
}




//using HelpDesk.Core.Common;
//using HelpDesk.Core.Interfaces;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using HelpDesk.Core.Enums;

//namespace HelpDesk.Infrastructure.Repositories.Implementations.Service
//{
//    public class LookupService : ILookupService
//    {
//        public async Task<ApiResponse<List<string>>> GetPrioritiesAsync()
//        {
//            var priorities = Enum.GetNames(typeof(TicketPriority)).ToList();
//            return await Task.FromResult(ApiResponse<List<string>>.Success(priorities));
//        }

//        public async Task<ApiResponse<List<string>>> GetStatusesAsync()
//        {
//            var statuses = Enum.GetNames(typeof(TicketStatus)).ToList();
//            return await Task.FromResult(ApiResponse<List<string>>.Success(statuses));
//        }
//    }
//}
