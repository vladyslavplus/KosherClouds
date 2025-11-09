namespace KosherClouds.OrderService.Mapping;
using AutoMapper;
using KosherClouds.OrderService.Entities;
using KosherClouds.OrderService.DTOs.PaymentRecord;

public class PaymentRecordProfile : Profile
{
    public PaymentRecordProfile()
    {
        CreateMap<PaymentRecord, PaymentRecordResponseDto>();

        CreateMap<PaymentRecordCreateDto, PaymentRecord>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Order, opt => opt.Ignore())
            .ForMember(dest => dest.OrderId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
    }
}
    