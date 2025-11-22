using AutoMapper;
using KosherClouds.BookingService.DTOs;
using KosherClouds.BookingService.Entities;

namespace KosherClouds.BookingService.Mapping
{
    public class BookingProfile : Profile
    {
        public BookingProfile()
        {
            CreateMap<BookingCreateDto, Booking>()
                .ForMember(dest => dest.Hookahs, opt => opt.MapFrom(src => src.Hookahs ?? new List<HookahBookingDto>()))
                .ForMember(dest => dest.Zone, opt => opt.MapFrom(src => Enum.Parse<BookingZone>(src.Zone, true)))
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            CreateMap<HookahBookingDto, HookahBooking>()
                .ForMember(dest => dest.Strength, opt => opt.MapFrom(src => Enum.Parse<HookahStrength>(src.Strength, true)))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.BookingId, opt => opt.Ignore())
                .ForMember(dest => dest.Booking, opt => opt.Ignore());

            CreateMap<Booking, BookingResponseDto>()
                .ForMember(dest => dest.Zone, opt => opt.MapFrom(src => src.Zone.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<HookahBooking, HookahBookingDto>()
                .ForMember(dest => dest.Strength, opt => opt.MapFrom(src => src.Strength.ToString()));
        }
    }
}