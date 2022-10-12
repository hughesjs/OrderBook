namespace OrderBookService.Models;

internal abstract record Order
{
	public required decimal     Price          { get; init; }
	public required OrderAction OrderAction    { get; init; }
}
