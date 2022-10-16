using OrderBookService.Domain.Models.Assets;

namespace OrderBookService.Application.Exceptions;

public class FailedToModifyOrDeleteOrderException : Exception
{
	private Guid            OrderId { get; }
	private AssetDefinition Asset   { get; }

	public FailedToModifyOrDeleteOrderException(string? message, string orderId, AssetDefinition asset) : base(message)
	{
		OrderId = Guid.Parse(orderId);
		Asset   = asset;
	}
}
