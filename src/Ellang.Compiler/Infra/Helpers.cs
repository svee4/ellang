namespace Ellang.Compiler.Infra;

public static class Helpers
{
	public static string GetPrettyTypeName(Type type) =>
		!type.IsGenericType
			? type.Name
			: $"{type.Name.Split('`')[0]}<{string.Join(", ", type.GenericTypeArguments.Select(GetPrettyTypeName))}>";

	public static string GetPrettyTypeName<T>() => GetPrettyTypeName(typeof(T));
}
