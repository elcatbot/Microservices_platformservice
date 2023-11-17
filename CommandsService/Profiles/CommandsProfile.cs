using AutoMapper;
using CommandsService.DTOs;
using CommandsService.Models;
using PlatformService;

namespace CommandsService.Profiles
{
    public class CommandsProfile : Profile
    {
        public CommandsProfile()
        {
            // Source -> Target
            CreateMap<Platform, PlatformReadDto>();
            CreateMap<CommandCreateDto, Command>();
            CreateMap<Command, CommandReadDto>();
            CreateMap<PlatformPublishedDto, Platform>().ForMember(desti => desti.ExternalId, opt => opt.MapFrom(src => src.Id));
            CreateMap<GrpcPlatformModel, Platform>()
                .ForMember(desti => desti.ExternalId, opt => opt.MapFrom(src => src.PlatformId))
                .ForMember(desti => desti.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(desti => desti.Commands, opt => opt.Ignore());
        }
    }
}