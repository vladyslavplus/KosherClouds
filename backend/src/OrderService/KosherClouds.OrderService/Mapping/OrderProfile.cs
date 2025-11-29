using AutoMapper;
using KosherClouds.OrderService.Entities;
using KosherClouds.OrderService.DTOs.Order;

namespace KosherClouds.OrderService.Mapping
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<Order, OrderResponseDto>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

            CreateMap<OrderCreateDto, Order>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ContactName, opt => opt.MapFrom(src => src.ContactName))
                .ForMember(dest => dest.ContactPhone, opt => opt.MapFrom(src => src.ContactPhone))
                .ForMember(dest => dest.ContactEmail, opt => opt.MapFrom(src => src.ContactEmail))
                .ForMember(dest => dest.PaymentType, opt => opt.MapFrom(src => src.PaymentType))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));
        }
    }
}