namespace OrderBookService.Domain.Models.AssetClasses;

public abstract record AssetClassBase
{
	public abstract string Symbol { get; }
}
