using AutoMapper;
using OrderBookService.Domain.Entities;
namespace OrderBookService.Application.Mapping;

public class EntityMappingProfile: Profile
{
	public EntityMappingProfile()
	{
		CreateMap<Domain.Models.OrderBooks.OrderBook, OrderBookEntity>().ReverseMap();
	}
}
