namespace OrderBookService.Domain.Models.Assets;

public class AssetDefinition
{
	public required AssetClass Class { get; init; }
	public required string     Symbol     { get; init; }
}
