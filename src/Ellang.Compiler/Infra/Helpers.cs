using Ellang.Compiler.Parser.Nodes;

namespace Ellang.Compiler.Infra;

public static class Helpers
{
	public static string GetPrettyTypeName(Type type) =>
		!type.IsGenericType
			? type.Name
			: $"{type.Name.Split('`')[0]}<{string.Join(", ", type.GenericTypeArguments.Select(GetPrettyTypeName))}>";

	public static string GetPrettyTypeName<T>() => GetPrettyTypeName(typeof(T));

	public static string TypeRefSyntaxToString(TypeRef type)
	{
		var b = $"{new string('&', type.PointerCount)}{type.Identifier.Value}";

		if (type.Generics.Count > 0)
		{
			b += $"<{string.Join(", ", type.Generics.Select(TypeRefSyntaxToString))}>";
		}

		return b;
	}
}
