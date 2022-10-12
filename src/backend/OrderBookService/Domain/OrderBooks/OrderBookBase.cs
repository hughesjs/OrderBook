using System.Collections;

namespace OrderBookService.Models;

internal abstract class OrderBookBase<TAsset>:  ICollection<Order>
{
	public abstract TAsset UnderlyingAsset { get; }
	public abstract IReadOnlyCollection<Order> Orders { get; }

	public abstract IEnumerator<Order> GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	public abstract void Add(Order item);
	public abstract void Clear();
	public abstract bool Contains(Order item);
	public abstract void CopyTo(Order[] array, int arrayIndex);
	public abstract bool Remove(Order item);
	public abstract int  Count      { get; }
	public abstract bool IsReadOnly { get; }
}
