using AutoMapper;
using Web.Services.CouponAPI.Models;
using Web.Services.CouponAPI.Models.Dto;

namespace Web.Services.CouponAPI
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(config =>
            {
                config.CreateMap<CouponDto, Coupon>().ReverseMap();
            });
            return mappingConfig;
        }
    }
}
