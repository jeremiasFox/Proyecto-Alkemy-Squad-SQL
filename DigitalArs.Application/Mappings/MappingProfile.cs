using AutoMapper;
using DigitalArs.Application.DTOs.User;
using DigitalArs.Domain.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DigitalArs.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserResponseDto>()
            .ForMember(d => d.RoleName, opt => opt.MapFrom(s => s.Role.Name));

        CreateMap<UserCreateRequestDto, User>()
    .ForMember(d => d.Password, opt => opt.Ignore());
    }
}