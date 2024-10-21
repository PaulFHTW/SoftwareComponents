using AutoMapper;

namespace RestAPI.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<DTO.DocumentDTO, DAL.Entities.Document>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Random.Shared.Next()))
            .ForMember(dest => dest.UploadDate, opt => opt.MapFrom(_ => DateTime.Now))
            .ForMember(dest => dest.Path, opt => opt.MapFrom(_ => "template"))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.File.FileName));
    }
}

