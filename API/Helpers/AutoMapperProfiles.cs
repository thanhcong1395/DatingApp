using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<AppUser, MemberDto>()
                .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => src.Photos.FirstOrDefault(e => e.IsMain).Url))
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge()));
            CreateMap<Photo, PhotoDto>();
            CreateMap<MemberUpdateDto, AppUser>();
            CreateMap<RegisterDto, AppUser>();
            CreateMap<Message, MessageDto>()
                .ForMember(e => e.SenderPhotoUrl, e => e.MapFrom(x => x.Sender.Photos.FirstOrDefault(w => w.IsMain).Url))
                .ForMember(e => e.RecipientPhotoUrl, e => e.MapFrom(x => x.Recipient.Photos.FirstOrDefault(w => w.IsMain).Url));
        }
    }
}