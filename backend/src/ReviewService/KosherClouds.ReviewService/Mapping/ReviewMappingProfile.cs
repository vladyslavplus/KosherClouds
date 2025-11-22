using AutoMapper;
using KosherClouds.ReviewService.DTOs;
using KosherClouds.ReviewService.Entities;

namespace KosherClouds.ReviewService.Mapping
{
    public class ReviewMappingProfile : Profile
    {
        public ReviewMappingProfile()
        {
            CreateMap<ReviewCreateDto, Review>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.IsVerifiedPurchase, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ReviewStatus.Published))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTimeOffset.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ModerationNotes, opt => opt.Ignore())
                .ForMember(dest => dest.ModeratedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModeratedAt, opt => opt.Ignore());

            CreateMap<Review, ReviewResponseDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.UserName, opt => opt.Ignore());
        }
    }
}
