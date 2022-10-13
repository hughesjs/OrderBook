using MongoDB.Bson.Serialization.Attributes;
using OrderBookService.Domain.Models.Assets;
using OrderBookService.Domain.Models.Orders;

namespace OrderBookService.Domain.Entities;


internal class OrderBookEntity
{
	[BsonId]
	public AssetDefinition UnderlyingAsset { get; init; }
	
	public List<Order> Orders { get; init; }
}
