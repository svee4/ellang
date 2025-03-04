using System.Diagnostics.CodeAnalysis;

namespace Ellang.Compiler.AST;

public abstract class SymbolTable<T>(SymbolTableManager manager) where T : GlobalSymbol
{
	protected SymbolTableManager Manager { get; } = manager;
	protected Dictionary<SymbolIdent, T> SymbolMap { get; } = [];

	public IReadOnlyList<T> Symbols => SymbolMap.Values.ToArray();

	public bool Contains(SymbolIdent name) => TryGet(name, out _);

	public bool TryGet(SymbolIdent ident, [NotNullWhen(true)] out T? symbol)
	{
		if (SymbolMap.TryGetValue(ident, out var sym))
		{
			symbol = sym;
			return true;
		}

		symbol = null;
		return false;
	}

	public void Add(T symbol)
	{
		Manager.EnsureSymbolNameIsUnique(symbol.Ident);
		SymbolMap.Add(symbol.Ident, symbol);
	}

	public T Get(SymbolIdent ident) =>
		SymbolMap[ident];

	public void Update(SymbolIdent ident, Func<T, T> updater) =>
		SymbolMap[ident] = updater(SymbolMap[ident]);
}

public sealed class VariableTable(SymbolTableManager manager) : SymbolTable<VariableSymbol>(manager);
public sealed class StructTable(SymbolTableManager manager) : SymbolTable<StructSymbol>(manager);
public sealed class TraitTable(SymbolTableManager manager) : SymbolTable<TraitSymbol>(manager);
public sealed class FunctionTable(SymbolTableManager manager) : SymbolTable<NamedFunctionSymbol>(manager);
