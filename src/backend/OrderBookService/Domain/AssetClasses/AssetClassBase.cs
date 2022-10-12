namespace OrderBookService.Models;

public abstract record AssetClassBase
{
	public abstract string Symbol { get; }
}
