using FluentValidation;
using JetBrains.Annotations;
using OrderBookProtos.ServiceBases;
using OrderBookService.Application.Misc;

namespace OrderBookService.Application.Validation;

[UsedImplicitly]
internal class GetPriceRequestValidator: AbstractValidator<GetPriceRequest>
{
	public GetPriceRequestValidator()
	{
		RuleFor(request => request.AssetDefinition).NotEmpty();
		RuleFor(request => request.Amount).NotEmpty();
		RuleFor(request => request.Amount).Must(dv => dv > 0).WithMessage(StaticStrings.AmountMustBeNonZeroPositiveRealNumberMessage);
		RuleFor(request => request.Amount).Custom(CustomValidationRules.DecimalValueIsValidDecimal);
		RuleFor(request => request.OrderAction).IsInEnum();
		RuleFor(request => request.AssetDefinition.Class).IsInEnum();
		RuleFor(request => request.AssetDefinition.Symbol).NotEmpty();
	}
}
