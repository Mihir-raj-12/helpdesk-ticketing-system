using AutoMapper;
using HelpDesk.Core.DTOs.Audit;
using HelpDesk.Core.DTOs.Category;
using HelpDesk.Core.DTOs.Comment;
using HelpDesk.Core.DTOs.Department;
using HelpDesk.Core.DTOs.Settings;
using HelpDesk.Core.DTOs.Ticket;
using HelpDesk.Core.DTOs.User;
using HelpDesk.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User mappings
            CreateMap<ApplicationUser, UserResponseDto>()
                .ForMember(dest => dest.Role, opt => opt.Ignore());

            // Ticket mappings
            CreateMap<Ticket, TicketResponseDto>()
     .ForMember(dest => dest.CategoryName,
         opt => opt.MapFrom(src => src.Category.Name))
     .ForMember(dest => dest.RaisedByUserName,
         opt => opt.MapFrom(src => src.RaisedByUser.FullName))
     .ForMember(dest => dest.AssignedToUserName,
         opt => opt.MapFrom(src => src.AssignedToUser != null
             ? src.AssignedToUser.FullName
             : null));

            CreateMap<CreateTicketDto, Ticket>();

            // Category mappings
            CreateMap<Category, CategoryResponseDto>();
            CreateMap<CreateCategoryDto, Category>();

            // Comment mappings
            CreateMap<Comment, CommentResponseDto>()
                .ForMember(dest => dest.WrittenByUserName,
                    opt => opt.MapFrom(src => src.User.FullName));
            CreateMap<CreateCommentDto, Comment>();

            // --- NEW: Department Mappings ---
            CreateMap<Department, DepartmentResponseDto>()
                .ForMember(dest => dest.DepartmentHeadName,
                    opt => opt.MapFrom(src => src.DepartmentHead != null ? src.DepartmentHead.FullName : "Unassigned"))
                .ForMember(dest => dest.ActiveUserCount,
                    opt => opt.MapFrom(src => src.Users.Count(u => u.IsActive)));

            CreateMap<CreateDepartmentDto, Department>();

            // --- NEW: System Settings Mapping ---
            CreateMap<SystemSetting, SystemSettingDto>().ReverseMap();

            // User mappings
            CreateMap<ApplicationUser, UserResponseDto>()
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                // NEW: Map the DepartmentName so Angular gets a clean string
                .ForMember(dest => dest.DepartmentName,
                    opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : "Unassigned"));

            // Audit Mappings
            CreateMap<AuditDetail, AuditDetailDto>();

            CreateMap<AuditLog, AuditLogResponseDto>()
                .ForMember(dest => dest.Details, opt => opt.MapFrom(src => src.AuditDetails))
                // Flatten the user's name from the Navigation property!
                .ForMember(dest => dest.PerformedByUserName,
                    opt => opt.MapFrom(src => src.PerformedByUser != null ? src.PerformedByUser.FullName : "System"));
        }
    }
}
