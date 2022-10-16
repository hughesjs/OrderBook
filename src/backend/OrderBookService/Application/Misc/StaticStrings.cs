namespace OrderBookService.Application.Misc;

/// <summary>
/// This isn't perfect, but it means I can avoid coupling various classes to eachother
/// just for the sake of sharing some string constants. In the real world, it might be
/// prudent to replace this with some sort of string provider service that could, if
/// necessary also handle localisation 
/// </summary>
public static class StaticStrings
{
	public const string IdempotencyPrefix                         = "idempotency";
	
	public const string NoIdempotencyKeyProvidedMessage              = "This operation should be idempotent, but no idempotency key was provided";
	public const string IdempotentOperationAlreadyCompleteMessage    = "This request is being ignored as a previous request has been processed with this idempotency key";
	public const string SuccessMessage                               = "Success";
	public const string UnsatisfiableOrderMessage                    = "Unsatisfiable order";
    public const string FailedToDeleteNoOrderBookMessage             = "Failed to delete order, OrderBook does not exist";
	public const string FailedToDeleteOrderIdNonExistent             = "Failed to delete order, OrderId does not exist";
	public const string AmountMustBeNonZeroPositiveRealNumberMessage = "Amount is invalid (must be > 0)";

	public const string MandatoryFieldNotPresentMessageTemplate = "{0} is mandatory but was not present in request";

}
