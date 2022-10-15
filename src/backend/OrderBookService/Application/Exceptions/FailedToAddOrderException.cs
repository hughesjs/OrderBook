using OrderBookService.Domain.Entities;
using OrderBookService.Domain.Models.Assets;

namespace OrderBookService.Exceptions;

public class FailedToAddOrderException: Exception
{
	public AssetDefinition Asset { get; }
	public OrderEntity     OrderEntity { get; }
	
	public FailedToAddOrderException(string? message, AssetDefinition asset, OrderEntity orderEntity) : base(message)
	{
		Asset       = asset;
		OrderEntity = orderEntity;
	}
}
