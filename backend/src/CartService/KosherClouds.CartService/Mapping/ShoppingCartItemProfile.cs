namespace KosherClouds.CartService.Mapping;

using KosherClouds.CartService.DTOs;
using AutoMapper;
using KosherClouds.CartService.Entities;

public class ShoppingCartItemProfile : Profile
{
    public ShoppingCartItemProfile()
    {
        CreateMap<ShoppingCartItem, CartItemDto>();
        
    }
}