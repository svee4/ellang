using Ellang.Compiler.Parser.Nodes;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Ellang.Compiler.Infra;

[CollectionBuilder(typeof(EquatableListExtensions), "Create")]
public sealed class EquatableList<T> : IEquatable<EquatableList<T>>, IList<T>
{
	private readonly IList<T> _list;

	public EquatableList(IEnumerable<T> items) => _list = [.. items];
	public EquatableList(ReadOnlySpan<T> items) => _list = [.. items];

	public bool Equals(EquatableList<T>? other) => other is not null && _list.SequenceEqual(other);
	public override bool Equals(object? obj) => Equals(obj as EquatableList<T>);

	public override int GetHashCode()
	{
		var code = new HashCode();
		foreach (var value in _list)
		{
			code.Add(value);
		}

		return code.ToHashCode();
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", 
		Justification = "ITS AN EXTENSION METHOD")]
	public static implicit operator EquatableList<T>(List<T> source) => new(source);

	[Obsolete("Use EquatableList")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2225:Operator overloads have named alternates",
		Justification = "ITS AN EXTENSION METHOD")]
	public static implicit operator EquatableList<T>(NodeList<T> source) => new(source.Nodes);

	public override string ToString() => string.Join(", ", _list.Select(v => v?.ToString()));

	public T this[int index] { get => _list[index]; set => _list[index] = value; }

	public int Count => _list.Count;

	public bool IsReadOnly => _list.IsReadOnly;

	public void Add(T item) => _list.Add(item);
	public void Insert(int index, T item) => _list.Insert(index, item);

	public void Clear() => _list.Clear();

	public bool Contains(T item) => _list.Contains(item);
	public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

	public int IndexOf(T item) => _list.IndexOf(item);

	public bool Remove(T item) => _list.Remove(item);
	public void RemoveAt(int index) => _list.RemoveAt(index);

	public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_list).GetEnumerator();
}

public static class EquatableListExtensions
{
	public static EquatableList<T> ToEquatable<T>(this List<T> source) => source;

	[Obsolete("Use EquatableList")]
	public static EquatableList<T> ToEquatable<T>(this NodeList<T> source) => source;

	public static EquatableList<T> Create<T>(ReadOnlySpan<T> items) => new(items);
}
