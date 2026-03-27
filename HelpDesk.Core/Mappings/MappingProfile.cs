using AutoMapper;
using HelpDesk.Core.DTOs.Category;
using HelpDesk.Core.DTOs.Comment;
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
                        : null))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Priority,
                    opt => opt.MapFrom(src => src.Priority.ToString()));

            CreateMap<CreateTicketDto, Ticket>();

            // Category mappings
            CreateMap<Category, CategoryResponseDto>();
            CreateMap<CreateCategoryDto, Category>();

            // Comment mappings
            CreateMap<Comment, CommentResponseDto>()
                .ForMember(dest => dest.WrittenByUserName,
                    opt => opt.MapFrom(src => src.User.FullName));
            CreateMap<CreateCommentDto, Comment>();
        }
    }
}
