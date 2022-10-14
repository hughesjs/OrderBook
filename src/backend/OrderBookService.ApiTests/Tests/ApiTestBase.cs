using AutoFixture;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using OrderBookProtos.CustomTypes;
using Xunit.Abstractions;

namespace OrderBookService.ApiTests;

public class ApiTestBase : IClassFixture<OrderBookTestFixture>, IDisposable
{
	private OrderBookTestFixture Fixture { get; }
	
	private   LoggerFactory        LoggerFactory { get; }
	private   GrpcChannel?         _channel;

	protected static Fixture AutoFixture { get; } = new();

	static ApiTestBase()
	{
		AutoFixture.Customize<GuidValue>(c => c.With(p => p.Value, () => Guid.NewGuid().ToString()));
	}
	
	
	protected ApiTestBase(OrderBookTestFixture fixture, ITestOutputHelper outputHelper)
	{
		LoggerFactory = new();
		Fixture       = fixture;
	}
	
	protected GrpcChannel Channel => _channel ??= CreateChannel();

	private GrpcChannel CreateChannel()
	{
		return GrpcChannel.ForAddress("http://localhost", new()
														  {
															  LoggerFactory = LoggerFactory,
															  HttpHandler   = Fixture.Handler
														  });
	}

	public void Dispose()
	{
		_channel = null;
	}
}
