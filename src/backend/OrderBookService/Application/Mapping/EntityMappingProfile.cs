using AutoMapper;
using JetBrains.Annotations;
using OrderBookProtos.ServiceBases;
using OrderBookService.Domain.Entities;
using OrderBookService.Domain.Models.OrderBooks;
using OrderBookService.Domain.Models.Orders;

namespace OrderBookService.Application.Mapping;

[UsedImplicitly]
public class EntityMappingProfile: Profile
{
	public EntityMappingProfile()
	{
		CreateMap<OrderBook, OrderBookEntity>()
		   .ForMember(dest => dest.Orders, opt => opt.MapFrom(s => s.Orders.ToHashSet()));

		CreateMap<OrderBookEntity, OrderBook>()
		   .ForMember(dest => dest.Orders, opt => opt.MapFrom(s => s.Orders.AsReadOnly()));

		CreateMap<Order, OrderEntity>().ReverseMap();
		CreateMap<AddOrModifyOrderRequest, OrderEntity>()
		   .ForMember(dest => dest.Id, opt => opt.MapFrom(s => s.OrderId.Value))
		   .ForMember(dest => dest.EffectiveTime, opt => opt.Ignore());
	}
}
