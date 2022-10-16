using FluentValidation;
using OrderBookProtos.CustomTypes;
using OrderBookProtos.ServiceBases;

namespace OrderBookService.Application.Validation;

public static class CustomValidationRules
{
	public static void DecimalValueIsValidDecimal(DecimalValue dv, ValidationContext<GetPriceRequest> ctx) => ValidateDecimalValue(dv, ctx);

	public static void DecimalValueIsValidDecimal(DecimalValue dv, ValidationContext<AddOrderRequest> ctx) => ValidateDecimalValue(dv, ctx);
	
	public static void DecimalValueIsValidDecimal(DecimalValue dv, ValidationContext<ModifyOrderRequest> ctx) => ValidateDecimalValue(dv, ctx);

	private static void ValidateDecimalValue<TCtx>(DecimalValue dv, ValidationContext<TCtx> ctx)
	{
		if (dv.Nanos < 0 != dv.Units <= 0)
		{
			ctx.AddFailure("Sign on decimal nanos and units must match");
		}
	}
}
