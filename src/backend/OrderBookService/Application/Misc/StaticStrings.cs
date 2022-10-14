namespace OrderBookService.Application.Misc;

public class StaticStrings
{
	public const string IdempotencyPrefix                         = "idempotency";
	
	public const string NoIdempotencyKeyProvidedMessage           = "This operation should be idempotent, but no idempotency key was provided";
	public const string IdempotentOperationAlreadyCompleteMessage = "This request is being ignored as a previous request has been processed with this idempotency key";
	public const string SuccessMessage                            = "Success";
	public const string UnsatisfiableOrderMessage                 = "Unsatisfiable order";
    public const string FailedToDeleteNoOrderBookMessage		  = "Failed to delete order, OrderBook does not exist";
	public const string FailedToDeleteOrderIdNonExistant		  = "Failed to delete order, OrderId does not exist";

}
