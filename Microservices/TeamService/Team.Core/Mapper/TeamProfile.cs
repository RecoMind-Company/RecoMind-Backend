using AutoMapper;
using System.Net.Mail;
using Team.Core.DTOs;
using Team.Core.Models;


namespace Team.Core.Mapper
{
    public class TeamProfile : Profile
    {
        public TeamProfile()
        {
            CreateMap<TeamModel, TeamResponseForAiDto>();

            CreateMap<TeamModel, TeamResponseDto>()
                .ForMember(dest => dest.Employees, opt => opt.MapFrom(src => src.TeamEmployees.Select(e => e.EmployeeId).ToList()));

            CreateMap<CreateTeamDto, TeamModel>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CompanyId, opt => opt.Ignore())
                .ForMember(dest => dest.TeamEmployees, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            CreateMap<UpdateTeamDto, TeamModel>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CompanyId, opt => opt.Ignore())
                .ForMember(dest => dest.TeamEmployees, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}