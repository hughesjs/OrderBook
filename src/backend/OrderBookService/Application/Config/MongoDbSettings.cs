namespace OrderBookService.Application.Config;

public class MongoDbSettings
{
	public required string DatabaseName { get; init; }
	public required string ConnectionString { get; init; }
}
