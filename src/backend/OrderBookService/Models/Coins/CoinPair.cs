namespace OrderBookService.Models;

public record struct CoinPair
{
	// It might be worth replacing string with a CoinDefinition class or something down the line
	// For the sake of this exercise, this is more than sufficient though
	public required string CoinOne;
	public required string CoinTwo;

	public override string ToString() => $"{CoinOne}{CoinTwo}".ToUpperInvariant();
}
