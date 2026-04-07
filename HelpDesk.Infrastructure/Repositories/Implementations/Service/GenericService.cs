using AutoMapper;
using HelpDesk.Core.Common;
using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Infrastructure.Repositories.Implementations.Service
{
    public class GenericService<TModel, TEntity> : IGenericService<TModel>
        where TModel : class
        where TEntity : BaseEntity
    {
        protected readonly IMapper _mapper;
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly ICurrentUserProvider _currentUserProvider;
        protected readonly IGenericRepository<TEntity> _repository;

        public GenericService(
            IMapper mapper,
            IUnitOfWork unitOfWork,
            ICurrentUserProvider currentUserProvider,
            IGenericRepository<TEntity> repository)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _currentUserProvider = currentUserProvider;
            _repository = repository;
        }

        public virtual async Task<ApiResponse<TModel>> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<TModel>.Failure($"{typeof(TEntity).Name} not found.");

            var responseDto = _mapper.Map<TModel>(entity);
            return ApiResponse<TModel>.Success(responseDto);
        }

        public virtual async Task<ApiResponse<List<TModel>>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            var responseDto = _mapper.Map<List<TModel>>(entities);
            return ApiResponse<List<TModel>>.Success(responseDto);
        }

        public virtual async Task<ApiResponse<TModel>> CreateAsync(object createDto)
        {
            // Map the incoming CreateDto to the Entity
            var entity = _mapper.Map<TEntity>(createDto);

            // Add to database
            await _repository.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            // Map the saved Entity back to a ResponseDto
            var responseDto = _mapper.Map<TModel>(entity);
            return ApiResponse<TModel>.Success(responseDto, $"{typeof(TEntity).Name} created successfully.");
        }

        public virtual async Task<ApiResponse<bool>> DeactivateAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return ApiResponse<bool>.Failure($"{typeof(TEntity).Name} not found.");

            await _repository.SoftDeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.Success(true, $"{typeof(TEntity).Name} deactivated successfully.");
        }
    }
}
