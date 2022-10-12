namespace OrderBookService.Models;

internal sealed class CoinPairOrderBook: OrderBookBase<CoinPair>
{
	public override CoinPair UnderlyingAsset { get; }
	public override IReadOnlyCollection<Order> Orders { get; }

	private readonly List<Order> _orders;

	public CoinPairOrderBook(CoinPair underlyingAsset)
	{
		UnderlyingAsset = underlyingAsset;
		_orders         = new();
	}

	public override int  Count      => _orders.Count;
	
	public override bool IsReadOnly => false;
	
	public override IEnumerator<Order> GetEnumerator() => Orders.GetEnumerator();

	public override void Add(Order item) => _orders.Add(item);

	public override void Clear() => _orders.Clear();

	public override bool Contains(Order item) => _orders.Contains(item);

	public override void CopyTo(Order[] array, int arrayIndex) => _orders.CopyTo(array, arrayIndex);

	public override bool Remove(Order item) => _orders.Remove(item);
}
