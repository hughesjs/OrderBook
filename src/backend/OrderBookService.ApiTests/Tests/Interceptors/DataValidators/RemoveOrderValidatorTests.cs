using AutoFixture;
using Grpc.Core;
using OrderBookProtos.CustomTypes;
using OrderBookProtos.ServiceBases;
using OrderBookService.ApiTests.TestInfrastructure;
using Shouldly;
using Xunit.Abstractions;

namespace OrderBookService.ApiTests.Tests.Interceptors.DataValidators;

public class RemoveOrderValidatorTests: ApiTestBase
{
	
	private const int NumTests = 100;

	private readonly OrderBookProtos.ServiceBases.OrderBookService.OrderBookServiceClient _client;

	public RemoveOrderValidatorTests(OrderBookTestFixture testFixture, ITestOutputHelper outputHelper) : base(testFixture, outputHelper)
	{
		_client = new(Channel);
	}
	
		
	[Fact]
	public async Task FailsWhenNoIdempotencyKeyIsProvided()
	{
		RemoveOrderRequest request = AutoFix.Build<RemoveOrderRequest>()
													 .With(r => r.IdempotencyKey, (GuidValue)null!)
													 .Create();

		RpcException resException = await Should.ThrowAsync<RpcException>(async () => await _client.RemoveOrderAsync(request));
		
		resException.StatusCode.ShouldBe(StatusCode.InvalidArgument);
		resException.Message.ShouldContain(nameof(request.IdempotencyKey));
	}
	
	[Fact]
	public async Task FailsWhenNoOrderIdIsProvided()
	{
		RemoveOrderRequest request = AutoFix.Build<RemoveOrderRequest>()
												.With(r => r.OrderId, (GuidValue)null!)
												.Create();

		RpcException resException = await Should.ThrowAsync<RpcException>(async () => await _client.RemoveOrderAsync(request));
		
		resException.StatusCode.ShouldBe(StatusCode.InvalidArgument);
		resException.Message.ShouldContain(nameof(request.OrderId));
	}
	
	[Fact]
	public async Task FailsIfAssetClassIsInvalid()
	{
		RemoveOrderRequest request = AutoFix.Create<RemoveOrderRequest>();
		request.AssetDefinition.Class = (AssetClass)int.MaxValue;

		RpcException resException = await Should.ThrowAsync<RpcException>(async () => await _client.RemoveOrderAsync(request));
		
		resException.StatusCode.ShouldBe(StatusCode.InvalidArgument);
		resException.Message.ShouldContain(nameof(request.AssetDefinition.Class));
	}
	
	[Fact]
	public async Task FailsIfAssetClassSymbolIsEmpty()
	{
		RemoveOrderRequest request = AutoFix.Create<RemoveOrderRequest>();
		request.AssetDefinition.Symbol = "";
		
		RpcException resException = await Should.ThrowAsync<RpcException>(async () => await _client.RemoveOrderAsync(request));
		
		resException.StatusCode.ShouldBe(StatusCode.InvalidArgument);
		resException.Message.ShouldContain(nameof(request.AssetDefinition.Symbol));
	}
}
