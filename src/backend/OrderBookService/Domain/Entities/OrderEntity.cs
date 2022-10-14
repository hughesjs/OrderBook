using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using OrderBookProtos.ServiceBases;

namespace OrderBookService.Domain.Entities;

public record OrderEntity
{
	[BsonId]
	[BsonRepresentation(BsonType.String)]
	public required string		Id			  { get; init; }
	public required decimal     Price         { get; init; }
	public required decimal     Amount        { get; init; }
	public required OrderAction OrderAction   { get; init; }
	public required DateTime    EffectiveTime { get; init; }
}
