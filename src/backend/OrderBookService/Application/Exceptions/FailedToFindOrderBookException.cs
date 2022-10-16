using OrderBookService.Domain.Models.Assets;

namespace OrderBookService.Application.Exceptions;

public class FailedToFindOrderBookException: Exception
{
	public AssetDefinition Asset { get; }
	public FailedToFindOrderBookException(AssetDefinition asset) : base("Could not find OrderBook for asset") => Asset = asset;
}
