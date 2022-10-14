using MongoDB.Bson.Serialization.Attributes;
using OrderBookService.Domain.Models.Assets;

namespace OrderBookService.Domain.Entities;


internal record OrderBookEntity
{
	[BsonId]
	public required AssetDefinition UnderlyingAsset { get; init; }
	
	public required List<OrderEntity> Orders { get; init; }
}
