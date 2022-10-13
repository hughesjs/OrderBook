using JetBrains.Annotations;
using OrderBookService.DependencyInjection;
using OrderBookService.Services.ProtosServices;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
builder.Services.AddOrderBookServices();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterProtosService>();
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
