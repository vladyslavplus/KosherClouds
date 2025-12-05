using AutoMapper;
using KosherClouds.ReviewService.DTOs;
using KosherClouds.ReviewService.Entities;

namespace KosherClouds.ReviewService.Mapping
{
    public class ReviewMappingProfile : Profile
    {
        public ReviewMappingProfile()
        {
            CreateMap<Review, ReviewResponseDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.ReviewType, opt => opt.MapFrom(src => src.ReviewType.ToString()))
                .ForMember(dest => dest.ProductName, opt => opt.Ignore())
                .ForMember(dest => dest.ProductNameUk, opt => opt.Ignore())
                .ForMember(dest => dest.UserName, opt => opt.Ignore());

            CreateMap<ReviewCreateDto, Review>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.IsVerifiedPurchase, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ModerationNotes, opt => opt.Ignore())
                .ForMember(dest => dest.ModeratedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModeratedAt, opt => opt.Ignore());
        }
    }
}