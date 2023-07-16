using ApplicationAuth.Common.Extensions;
using ApplicationAuth.Domain.Entities.Identity;
using ApplicationAuth.Domain.Entities.Product;
using ApplicationAuth.Models.RequestModels;
using ApplicationAuth.Models.ResponseModels;
using ApplicationAuth.Models.ResponseModels.Order;
using ApplicationAuth.Models.ResponseModels.Product;
using ApplicationAuth.Models.ResponseModels.Session;
using ApplicationAuth.Models.ResponseModels.Shop;
using ApplicationAuth.Services.Interfaces;
using Profile = ApplicationAuth.Domain.Entities.Identity.Profile;

namespace ApplicationAuth.Services.StartApp
{
    public class AutoMapperProfileConfiguration : AutoMapper.Profile
    {
        public AutoMapperProfileConfiguration()
        : this("MyProfile")
        {
        }

        protected AutoMapperProfileConfiguration(string profileName)
        : base(profileName)
        {

            CreateMap<UserDevice, UserDeviceResponseModel>()
                .ForMember(t => t.AddedAt, opt => opt.MapFrom(src => src.AddedAt.ToISO()));

            #region User model

            CreateMap<UserProfileRequestModel, Profile>()
                .ForMember(t => t.Id, opt => opt.Ignore())
                .ForMember(t => t.User, opt => opt.Ignore());

            CreateMap<Profile, UserResponseModel>()
                .ForMember(t => t.Email, opt => opt.MapFrom(x => x.User != null ? x.User.Email : ""))
                .ForMember(t => t.PhoneNumber, opt => opt.MapFrom(x => x.User != null ? x.User.PhoneNumber : ""))
                .ForMember(t => t.IsBlocked, opt => opt.MapFrom(x => x.User != null ? !x.User.IsActive : false));

            CreateMap<ApplicationUser, UserBaseResponseModel>()
               .IncludeAllDerived();

            CreateMap<ApplicationUser, UserResponseModel>()
                .ForMember(x => x.FirstName, opt => opt.MapFrom(x => x.Profile.FirstName))
                .ForMember(x => x.LastName, opt => opt.MapFrom(x => x.Profile.LastName))
                .ForMember(x => x.IsBlocked, opt => opt.MapFrom(x => !x.IsActive))
                .IncludeAllDerived();

            CreateMap<ApplicationUser, UserRoleResponseModel>();

            #endregion


            #region Product model

            CreateMap<Product, ProductResponseModel>()
                .ForMember(t => t.Id, opt => opt.MapFrom(x => x.Id))
                .ForMember(t => t.Name, opt => opt.MapFrom(x => x.Name))
                .ForMember(t => t.InStock, opt => opt.MapFrom(x => x.InStock))
                .ForMember(t => t.LastUpdate, opt => opt.MapFrom(x => x.LastUpdate))
                .ForMember(t => t.Price, opt => opt.MapFrom(x => x.Price))
                .ForMember(t => t.Image, opt => opt.MapFrom(x => x.Image.Path))
                .ForMember(t => t.Description, opt => opt.MapFrom(x => x.Description));

            CreateMap<Product, SmallProductResponseModel>()
                .ForMember(t => t.Id, opt => opt.MapFrom(x => x.Id))
                .ForMember(t => t.Name, opt => opt.MapFrom(x => x.Name))
                .ForMember(t => t.InStock, opt => opt.MapFrom(x => x.InStock))
                .ForMember(t => t.Image, opt => opt.MapFrom(x => x.Image.Path))
                .ForMember(t => t.Price, opt => opt.MapFrom(x => x.Price));

            #endregion

            #region Order model

            CreateMap<Order, OrderResponseModel>()
                .ForMember(t => t.Id, opt => opt.MapFrom(x => x.Id))
                .ForMember(t => t.UserId, opt => opt.MapFrom(x => x.UserId))
                .ForMember(t => t.Comment, opt => opt.MapFrom(x => x.Comment))
                .ForMember(t => t.Status, opt => opt.MapFrom(x => x.Status))
                .ForMember(t => t.TotalPrice, opt => opt.MapFrom(x => x.TotalPrice))
                .ForMember(t => t.CreatedAt, opt => opt.MapFrom(x => x.CreatedDate))
                .ForMember(t => t.Products, opt => opt.MapFrom(x => x.Products));

            CreateMap<ProductInOrder, ProductInOrderResponseModel>()
                .ForMember(t => t.Id, opt => opt.MapFrom(x => x.Id))
                .ForMember(t => t.OrderId, opt => opt.MapFrom(x => x.OrderId))
                .ForMember(t => t.Product, opt => opt.MapFrom(x => x.Product))
                .ForMember(t => t.Amount, opt => opt.MapFrom(x => x.Amount));

            #endregion
        }
    }
}
