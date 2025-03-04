using Ellang.Compiler.Lexer;

namespace Ellang.Compiler.Parser.Nodes;

public interface ITopLevelStatement;

public sealed record Identifier(string Value, string? Module)
{
	[Obsolete("Use constructor with Module")]
	public Identifier(string Value) : this(Value, null) { }
}

public record TypeRef(Identifier Identifier, int PointerCount, List<TypeRef> Generics);
public record KeywordTypeRef(string CoreLibType, Identifier Identifier) : TypeRef(Identifier, 0, []);

public sealed record NodeList<T>(List<T> Nodes)
{
	public override string ToString() => string.Join(", ", Nodes);
}

public static class NodeList
{
	public static NodeList<T> From<T>(IEnumerable<T> source) => new NodeList<T>(source.ToList());
}
