using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using OrderBookService.Domain.Models.AssetClasses;
using OrderBookService.Domain.Models.Orders;

namespace OrderBookService.Domain.Entities;


internal class OrderBookEntity<TAsset> where TAsset: AssetClassBase
{
	[BsonId]
	public TAsset AssetClass { get; init; }
	
	public List<Order> Orders { get; init; }
}
