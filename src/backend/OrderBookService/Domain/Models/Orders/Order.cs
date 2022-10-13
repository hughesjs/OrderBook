namespace OrderBookService.Domain.Models.Orders;

internal abstract record Order
{
	public required Guid        Id          { get; init; }
	public required decimal     Price       { get; init; }
	public required OrderAction OrderAction { get; init; }
}
