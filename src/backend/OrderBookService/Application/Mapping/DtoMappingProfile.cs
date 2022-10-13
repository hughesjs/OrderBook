using AutoMapper;
using OrderBook.Protos.CustomTypes;
using OrderBookService.Domain.Models.Assets;
using OrderBookService.Domain.Models.Orders;
using OrderBookService.Protos.ServiceBases;

namespace OrderBookService.Application.Mapping;

public class DtoMappingProfile: Profile
{
	public DtoMappingProfile()
	{
		CreateMap<GuidValue, Guid>().ReverseMap();
		CreateMap<AssetDefinitionValue, AssetDefinition>().ReverseMap();
		CreateMap<AddOrModifyOrderRequest, Order>()
		   .ForMember(dest => dest.Id, opt => opt.MapFrom(s => s.OrderId))
		   .ForMember(dest => dest.EffectiveTime, opt => opt.Ignore());
	}
}
