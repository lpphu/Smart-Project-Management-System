using AutoMapper;
using Application.DTOs;
using Domain.Entities;

namespace Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Team, TeamDto>()
            .ForMember(dest => dest.MemberIds, opt => opt.MapFrom(src => src.TeamMembers.Select(m => m.UserId)));
    }
}