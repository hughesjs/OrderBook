using MongoDB.Bson.Serialization.Attributes;
using OrderBookService.Domain.Models.Assets;
using OrderBookService.Domain.Models.Orders;

namespace OrderBookService.Domain.Entities;


internal class OrderBookEntity
{
	[BsonId]
	public required AssetDefinition UnderlyingAsset { get; init; }
	
	public required List<OrderEntity> Orders { get; init; }
}
