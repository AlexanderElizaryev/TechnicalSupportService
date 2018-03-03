using AutoMapper;
using TechnicalSupportService.Models;
using TechnicalSupportService.Repository.DTO;

namespace TechnicalSupportService.Areas
{
    internal class AutoMapperProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<EmployeeDTO, EmployeeModel>()
                .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.ID))
                .ForMember(dest => dest.Status,  opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.Type , opt => opt.MapFrom(src => src.Type))
                .ReverseMap();

            //Mapper.CreateMap<RequestModel, RequestDTO>().ReverseMap();

            Mapper.AssertConfigurationIsValid();
        }
    }
}