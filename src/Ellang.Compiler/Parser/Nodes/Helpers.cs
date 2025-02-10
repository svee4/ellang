namespace Ellang.Compiler.Parser.Nodes;

public interface ITopLevelStatement;

public sealed record Identifier(string Value);

public sealed record TypeRef(Identifier Identifier, int ReferenceCount, List<TypeRef> Generics);

public sealed record NodeList<T>(List<T> Nodes)
{
	public override string ToString() => string.Join(", ", Nodes);
}

public static class NodeList
{
	public static NodeList<T> From<T>(IEnumerable<T> source) => new NodeList<T>(source.ToList());
}
