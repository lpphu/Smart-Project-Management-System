using AutoMapper;
using Application.DTOs;
using Domain.Entities;
using Task = Domain.Entities.Task;
namespace Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Task, TaskDto>();
        CreateMap<TaskHistory, TaskHistoryDto>();
    }
}