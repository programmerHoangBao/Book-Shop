using AutoMapper;
using back_end.DTOs.Auths.Requests;
using back_end.DTOs.Emails.Requests;
using back_end.Kafka.Messages;

namespace back_end.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<SendOtpRequest, SendOtpEmailMessage>();
        }
    }
}
