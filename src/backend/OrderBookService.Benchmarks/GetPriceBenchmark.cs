using System.Diagnostics;
using AutoMapper;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using OrderBookProtos.ServiceBases;
using OrderBookService.Benchmarks.Shims;

namespace OrderBookService.Benchmarks;

public class GetPriceBenchmark
{
	private Domain.Services.OrderBookService _obs     = null!;
	private GetPriceRequest                  _request = null!;
	
	[Params(1, (int)1e2, (int)1e3, (int)1e4, (int)1e5, (int)1e6)]
	public int OrdersNeededToComplete { get; set; }

	private const decimal AmountWanted = 1e9m;


	[GlobalSetup]
	public void Setup()
	{
		_request = new()
        		   {
        			   Amount = 1e9m
        		   };
        
        _obs = new( GetMapper(), new OrderBookShim(AmountWanted, OrdersNeededToComplete));
	}
	
	[Benchmark]
	public async Task Benchmark()
	{
		PriceResponse res = await _obs.GetPrice(_request);
		if (res.Price.Nanos == 0 && res.Price.Units == 0 || res.Price is null) throw new Exception("SHIT!");
	} 

	private IMapper GetMapper()
	{
		// This just saves manually building the IMapper in tests
		ServiceCollection services = new();
		services.AddAutoMapper(typeof(Program).Assembly);
		return services.BuildServiceProvider().GetRequiredService<IMapper>();
	}
}
