namespace KosherClouds.CartService.Mapping;
using KosherClouds.CartService.DTOs;
using AutoMapper;
using KosherClouds.CartService.Entities;

public class ShoppingCartProfile : Profile
{
    public ShoppingCartProfile()
    {
        CreateMap<ShoppingCart, ShoppingCartDto>().
             ForMember(dest => dest.GrandTotal, opt => opt.MapFrom(src => src.Items.Sum(i => i.Quantity * i.UnitPrice)))
             .ForMember(dest => dest.HasUnavailableItems, opt => opt.MapFrom(src => src.Items.Any(i => !i.IsAvailable)))
            ;
    }
}