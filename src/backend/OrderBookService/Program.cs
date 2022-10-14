using JetBrains.Annotations;
using OrderBookService.Application.Interceptors;
using OrderBookService.DependencyInjection;
using OrderBookService.Services.ProtosServices;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc(c=>c.Interceptors.Add<ExceptionInterceptor>());
builder.Services.AddGrpcReflection();

builder.Services.AddOrderBookServices();

builder.ConfigureOrderBookServices();


WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<OrderBookProtosService>();

if (app.Environment.IsDevelopment())
{
	app.MapGrpcReflectionService();
}

app.Run();

namespace OrderBookService
{
	[UsedImplicitly]
	public partial class Program { }
}
