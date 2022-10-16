using System.Collections;
using OrderBookService.Domain.Models.Assets;
using OrderBookService.Domain.Models.Orders;

namespace OrderBookService.Domain.Models.OrderBooks;

/// <summary>
/// Collection representing an Order Book
/// Collection operations assume OrderId is the unique key
/// Other properties are not checked for Contains/Remove
/// All items in the collection must have a unique OrderId
/// </summary>
internal class OrderBook:  ICollection<Order>
{
	public required AssetDefinition UnderlyingAsset { get; init; }
	
	// Just using the dictionary to ensure OrderId uniqueness
	private Dictionary<Guid, Order>    _orders         { get; } 
	public  IReadOnlyCollection<Order> Orders          => _orders.Values;
	public  int                        Count           => Orders.Count;
	public  bool                       IsReadOnly      => false;

	public OrderBook(AssetDefinition underlyingAsset, HashSet<Order> orders)
	{
		_orders = new();
		foreach (Order order in orders)
		{
			_orders.Add(order.Id, order);
		}
		UnderlyingAsset = underlyingAsset;
	}
	
	public OrderBook()
	{
		_orders = new();
	}

	public IEnumerator<Order> GetEnumerator()                       => Orders.GetEnumerator();
	IEnumerator IEnumerable.  GetEnumerator()                       => GetEnumerator();
	public void               Add(Order item)                       => _orders.Add(item.Id, item);
	public void               Clear()                               => _orders.Clear();
	public bool               Contains(Order item)                  => _orders.Keys.Contains(item.Id);
	public void               CopyTo(Order[] array, int arrayIndex) => _orders.Values.CopyTo(array, arrayIndex);
	public bool               Remove(Order item)					=> _orders.Remove(item.Id);

}
