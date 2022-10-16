using AutoMapper;
using JetBrains.Annotations;
using OrderBookProtos.CustomTypes;
using OrderBookProtos.ServiceBases;
using OrderBookService.Domain.Models.Assets;
using OrderBookService.Domain.Models.Orders;

namespace OrderBookService.Application.Mapping;

[UsedImplicitly]
public class DtoMappingProfile: Profile
{
	public DtoMappingProfile()
	{
		CreateMap<GuidValue, Guid>().ReverseMap();
		CreateMap<AssetDefinitionValue, AssetDefinition>().ReverseMap();
		CreateMap<ModifyOrderRequest, Order>()
		   .ForMember(dest => dest.Id, opt => opt.MapFrom(s => s.OrderId.Value))
		   .ForMember(dest => dest.EffectiveTime, opt => opt.Ignore());
		CreateMap<AddOrderRequest, Order>()
		   .ForMember(dest => dest.Id,            opt => opt.Ignore())
		   .ForMember(dest => dest.EffectiveTime, opt => opt.Ignore());
	}
}
