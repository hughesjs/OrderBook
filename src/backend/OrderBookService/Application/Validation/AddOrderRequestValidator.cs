using FluentValidation;
using JetBrains.Annotations;
using OrderBookProtos.ServiceBases;
using OrderBookService.Application.Misc;

namespace OrderBookService.Application.Validation;

[UsedImplicitly]
public class AddOrderRequestValidator: AbstractValidator<AddOrderRequest>
{
	public AddOrderRequestValidator()
	{
		RuleFor(request => request.IdempotencyKey).NotEmpty();
		RuleFor(request => request.AssetDefinition).NotEmpty();
		RuleFor(request => request.AssetDefinition.Class).IsInEnum();
		RuleFor(request => request.AssetDefinition.Symbol).NotEmpty();
		RuleFor(request => request.Amount).NotEmpty();
		RuleFor(request => request.Amount).Must(dv => dv > 0).WithMessage(StaticStrings.AmountMustBeNonZeroPositiveRealNumberMessage);
		RuleFor(request => request.Amount).Custom(CustomValidationRules.DecimalValueIsValidDecimal);
		RuleFor(request => request.Price).NotEmpty();
		RuleFor(request => request.Price).Must(dv => dv > 0).WithMessage(StaticStrings.AmountMustBeNonZeroPositiveRealNumberMessage);
		RuleFor(request => request.Price).Custom(CustomValidationRules.DecimalValueIsValidDecimal);
		RuleFor(request => request.OrderAction).IsInEnum();
	}
}
