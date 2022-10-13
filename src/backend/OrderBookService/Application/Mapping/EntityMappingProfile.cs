using AutoMapper;
using OrderBookService.Domain.Entities;
using OrderBookService.Domain.Models.Orders;

namespace OrderBookService.Application.Mapping;

public class EntityMappingProfile: Profile
{
	public EntityMappingProfile()
	{
		CreateMap<Domain.Models.OrderBooks.OrderBook, OrderBookEntity>()
		   .ForMember(dest => dest.Orders, opt => opt.MapFrom(s => s.Orders.ToHashSet()))
		   .ReverseMap();

		CreateMap<Order, OrderEntity>().ReverseMap();
	}
}
