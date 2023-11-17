using AutoMapper;
using PlatformService.Dtos;
using PlatformService.Models;

namespace PlatformService.Profiles
{
    public class PlatformsProfile : Profile
    {
        public PlatformsProfile()
        {
            // Source -> Target
            CreateMap<Platform, PlatformReadDto>();       
            CreateMap<PlatformCreateDto, Platform>();    
            CreateMap<PlatformReadDto, PlatformPublishedDto>(); 
            CreateMap<Platform, GrpcPlatformModel>()
                .ForMember(desti => desti.PlatformId, opt => opt.MapFrom(scr => scr.Id))
                .ForMember(desti => desti.Name, opt => opt.MapFrom(scr => scr.Name))
                .ForMember(desti => desti.Publisher, opt => opt.MapFrom(scr => scr.Publisher));
        }
    }   
}