namespace KosherClouds.OrderService.Mapping;
using AutoMapper;
using KosherClouds.OrderService.Entities;
using KosherClouds.OrderService.DTOs.Order;

public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<Order, OrderResponseDto>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.Payments, opt => opt.MapFrom(src => src.Payments));

        CreateMap<OrderCreateDto, Order>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => "Pending")) 
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items)) 
            .ForMember(dest => dest.Payments, opt => opt.Ignore());

        CreateMap<OrderUpdateDto, Order>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Items, opt => opt.Ignore())
            .ForMember(dest => dest.Payments, opt => opt.Ignore());
    }
}