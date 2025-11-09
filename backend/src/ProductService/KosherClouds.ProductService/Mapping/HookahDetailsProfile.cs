namespace KosherClouds.ProductService.Mapping;

using AutoMapper;
using KosherClouds.ProductService.Entities;
using KosherClouds.ProductService.DTOs.Hookah;

public class HookahDetailsProfile : Profile
{
    public HookahDetailsProfile()
    {
        CreateMap<HookahDetails, HookahDetailsDto>().ReverseMap();
    }
}