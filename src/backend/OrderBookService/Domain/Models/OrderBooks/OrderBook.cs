using System.Collections;
using OrderBookService.Domain.Models.Assets;
using OrderBookService.Domain.Models.Orders;

namespace OrderBookService.Domain.Models.OrderBooks;

internal class OrderBook:  ICollection<Order>
{
	public required AssetDefinition UnderlyingAsset { get; init; }
	
	public required HashSet<Order> Orders { get; init; }

	public int  Count      => Orders.Count;
	public bool IsReadOnly => false;

	public IEnumerator<Order> GetEnumerator()                       => Orders.GetEnumerator();
	IEnumerator IEnumerable.  GetEnumerator()                       => GetEnumerator();
	public void               Add(Order item)                       => Orders.Add(item);
	public void               Clear()                               => Orders.Clear();
	public bool               Contains(Order item)                  => Orders.Contains(item);
	public void               CopyTo(Order[] array, int arrayIndex) => Orders.CopyTo(array, arrayIndex);
	public bool               Remove(Order   item) => Orders.Remove(item);

}
