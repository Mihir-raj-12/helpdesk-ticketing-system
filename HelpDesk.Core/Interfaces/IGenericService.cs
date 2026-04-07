using HelpDesk.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Interfaces
{
    public interface IGenericService<TModel> where TModel : class
    {
        Task<ApiResponse<TModel>> GetByIdAsync(int id);
        Task<ApiResponse<List<TModel>>> GetAllAsync();
        Task<ApiResponse<TModel>> CreateAsync(object createDto);
        Task<ApiResponse<bool>> DeactivateAsync(int id);
    }
}
