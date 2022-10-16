using FluentValidation;
using JetBrains.Annotations;
using OrderBookProtos.ServiceBases;

namespace OrderBookService.Application.Validation;

[UsedImplicitly]
public class RemoveOrderRequestValidator : AbstractValidator<RemoveOrderRequest>
{
	public RemoveOrderRequestValidator()
	{
		RuleFor(request => request.IdempotencyKey).NotEmpty();
		RuleFor(request => request.OrderId).NotEmpty();
		RuleFor(request => request.AssetDefinition).NotEmpty();
		RuleFor(request => request.AssetDefinition.Class).IsInEnum();
		RuleFor(request => request.AssetDefinition.Symbol).NotEmpty();
	}
	
}
