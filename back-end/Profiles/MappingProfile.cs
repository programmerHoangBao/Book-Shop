using AutoMapper;
using back_end.Documents;
using back_end.DTOs.Categories.Requests;
using back_end.DTOs.Categories.Responses;
using back_end.DTOs.Emails.Requests;
using back_end.DTOs.Stores.Requests;
using back_end.DTOs.Stores.Responses;
using back_end.DTOs.Users.Infos;
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
            CreateMap<CreateStoreRequest, StoreEntity>();
            CreateMap<StoreEntity, CreateStoreResponse>();
            CreateMap<UpdateStoreRequest, StoreEntity>();
            CreateMap<StoreEntity, UpdateStoreResponse>();
            CreateMap<StoreEntity, StoreResponse>();
            CreateMap<UserEntity, GetProfileResponse>();
            CreateMap<StoreEntity, StoreInfo>();
            CreateMap<CreateCategoryRequest, CategoryEntity>();
            CreateMap<UpdateCategoryRequest, CategoryEntity>()
                .ForMember(destination => destination.Id, options => options.MapFrom(source => source.CategoryId));
            CreateMap<CategoryEntity, CategoryResponse>();
            CreateMap<UpdateUserRequest, UserEntity>();
            CreateMap<UserEntity, UpdateUserResponse>();
            CreateMap<BookEntity, BookDocument>();
            CreateMap<BookDocumentMessage, BookEntity>();
            CreateMap<BookEntity, BookDocumentMessage>();
        }
    }
}
