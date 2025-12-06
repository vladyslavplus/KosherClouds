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
                .ForMember(dest => dest.BookingDateTime,
                    opt => opt.MapFrom(src => DateTime.SpecifyKind(src.BookingDateTime, DateTimeKind.Utc)))
                .ForMember(dest => dest.Hookahs,
                    opt => opt.MapFrom(src => src.Hookahs ?? new List<HookahBookingDto>()))
                .ForMember(dest => dest.Zone,
                    opt => opt.MapFrom(src => Enum.Parse<BookingZone>(src.Zone, true)))
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            CreateMap<HookahBookingDto, HookahBooking>()
                .ForMember(dest => dest.ProductId,
                    opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.ProductName ?? "Unknown"))
                .ForMember(dest => dest.ProductNameUk,
                    opt => opt.MapFrom(src => src.ProductNameUk))
                .ForMember(dest => dest.TobaccoFlavor,
                    opt => opt.MapFrom(src => src.TobaccoFlavor))
                .ForMember(dest => dest.TobaccoFlavorUk,
                    opt => opt.MapFrom(src => src.TobaccoFlavorUk))
                .ForMember(dest => dest.Strength,
                    opt => opt.MapFrom(src => Enum.Parse<HookahStrength>(src.Strength, true)))
                .ForMember(dest => dest.ServeAfterMinutes,
                    opt => opt.MapFrom(src => src.ServeAfterMinutes))
                .ForMember(dest => dest.Notes,
                    opt => opt.MapFrom(src => src.Notes))
                .ForMember(dest => dest.PriceSnapshot,
                    opt => opt.MapFrom(src => src.PriceSnapshot ?? 0m))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.BookingId, opt => opt.Ignore())
                .ForMember(dest => dest.Booking, opt => opt.Ignore());

            CreateMap<Booking, BookingResponseDto>()
                .ForMember(dest => dest.Zone,
                    opt => opt.MapFrom(src => src.Zone.ToString()))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<HookahBooking, HookahBookingDto>()
                .ForMember(dest => dest.ProductId,
                    opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.ProductName))
                .ForMember(dest => dest.ProductNameUk,
                    opt => opt.MapFrom(src => src.ProductNameUk))
                .ForMember(dest => dest.TobaccoFlavor,
                    opt => opt.MapFrom(src => src.TobaccoFlavor))
                .ForMember(dest => dest.TobaccoFlavorUk,
                    opt => opt.MapFrom(src => src.TobaccoFlavorUk))
                .ForMember(dest => dest.Strength,
                    opt => opt.MapFrom(src => src.Strength.ToString()))
                .ForMember(dest => dest.ServeAfterMinutes,
                    opt => opt.MapFrom(src => src.ServeAfterMinutes))
                .ForMember(dest => dest.Notes,
                    opt => opt.MapFrom(src => src.Notes))
                .ForMember(dest => dest.PriceSnapshot,
                    opt => opt.MapFrom(src => src.PriceSnapshot));
        }
    }
}