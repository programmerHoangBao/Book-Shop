using AutoMapper;
using back_end.DTOs.Auths.Requests;
using back_end.DTOs.Emails.Requests;
using back_end.DTOs.Users.Requests;
using back_end.DTOs.Users.Responses;
using back_end.Entities;
using back_end.Kafka.Messages;

namespace back_end.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<SendOtpRequest, SendOtpEmailMessage>();
            CreateMap<CreateUserRequest, UserEntity>();
            CreateMap<UserEntity, CreateUserResponse>();
        }
    }
}
