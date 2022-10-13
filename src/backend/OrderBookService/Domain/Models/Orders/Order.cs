using OrderBookService.Protos.ServiceBases;

namespace OrderBookService.Domain.Models.Orders;

internal record Order
{
	public required Guid        Id            { get; init; }
	public required decimal     Price         { get; init; }
	public required decimal     Amount        { get; init; }
	public required OrderAction OrderAction   { get; init; }
	public required DateTime    EffectiveTime { get; init; }
}
