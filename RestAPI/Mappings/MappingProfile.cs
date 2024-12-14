using AutoMapper;
using DAL.Entities;
using RestAPI.DTO;

namespace RestAPI.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<DocumentDTO, Document>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Random.Shared.Next()))
            .ForMember(dest => dest.UploadDate, opt => opt.MapFrom(_ => DateTime.Now))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.File!.FileName));
    }
}

