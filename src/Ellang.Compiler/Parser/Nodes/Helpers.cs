namespace Ellang.Compiler.Parser.Nodes;

public interface ITopLevelStatement;

public sealed record Identifier(string Value, string? Module)
{
	[Obsolete("Use constructor with Module")]
	public Identifier(string Value) : this(Value, null) { }

	public override string ToString() => $"{Module}::{Value}";

	public static Identifier CoreLib(string value) => new(value, Constants.CoreLibModuleName);
}

/// <summary>
/// References to keyworded types are fixed. The syntax <c>int</c> will be represented in this type as: <br/>
/// <code>
/// Identifier = "Core::Int32" <br/>
/// OriginalIdentifier = "int"
/// </code>
/// <br/>
/// Non-keyworded types will have <c>OriginalIdentifier</c> set to null
/// </summary>
public record TypeRef(Identifier Identifier, int PointerCount, List<TypeRef> Generics, string? OriginalIdentifier);

public sealed record NodeList<T>(List<T> Nodes)
{
	public override string ToString() => string.Join(", ", Nodes);
}

public static class NodeList
{
	public static NodeList<T> From<T>(IEnumerable<T> source) => new NodeList<T>(source.ToList());
}
