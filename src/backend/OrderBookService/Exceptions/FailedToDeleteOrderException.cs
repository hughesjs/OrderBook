using OrderBookService.Domain.Models.Assets;

namespace OrderBookService.Exceptions;

public class FailedToDeleteOrderException : Exception
{
	private Guid            OrderId { get; }
	private AssetDefinition Asset   { get; }

	public FailedToDeleteOrderException(string? message, string orderId, AssetDefinition asset) : base(message)
	{
		OrderId = Guid.Parse(orderId);
		Asset   = asset;
	}
}
