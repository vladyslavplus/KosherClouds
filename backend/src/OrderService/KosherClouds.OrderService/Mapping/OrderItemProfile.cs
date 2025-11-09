namespace KosherClouds.OrderService.Mapping;
using AutoMapper;
using KosherClouds.OrderService.Entities;
using KosherClouds.OrderService.DTOs.OrderItem;
public class OrderItemProfile : Profile
{
    public OrderItemProfile() {
        CreateMap<OrderItem, OrderItemResponseDto>();

        CreateMap<OrderItemCreateDto, OrderItem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Order, opt => opt.Ignore())
            .ForMember(dest => dest.OrderId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
    }
}