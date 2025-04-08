using AutoMapper;
using OnlineStore.Contracts;
using OnlineStore.DTOs;
using OnlineStore.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OnlineStore.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Product mappings
            CreateMap<ProductCreateDto, Product>();
            CreateMap<ProductUpdateDto, Product>();
            CreateMap<Product, ProductResponseDto>()
                .ForMember(dest => dest.CreatedAt,
                          opt => opt.MapFrom(src => src.CreatedDate.ToString("yyyy-MM-dd HH:mm")));

            // User mappings (пример)
            CreateMap<UserRegisterDto, User>();
            CreateMap<User, UserResponseDto>();
        }
    }
}