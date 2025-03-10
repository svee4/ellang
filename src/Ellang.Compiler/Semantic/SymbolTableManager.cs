using Ellang.Compiler.Semantic.Binding;
using System.Diagnostics.CodeAnalysis;

namespace Ellang.Compiler.Semantic;

public sealed class SymbolTableManager
{
	public GlobalVariableTable GlobalVariableTable { get; private set; }
	public StructTable GlobalStructTable { get; private set; }
	public TraitTable GlobalTraitTable { get; private set; }
	public FunctionTable GlobalFunctionTable { get; private set; }
	public LocalVariablesManager LocalVariablesManager { get; private set; }

	public SymbolTableManager()
	{
		GlobalVariableTable = new(this);
		GlobalStructTable = new(this);
		GlobalTraitTable = new(this);
		GlobalFunctionTable = new(this);
		LocalVariablesManager = new(this);
	}

	public bool SymbolExists(SymbolIdent ident) =>
		GlobalVariableTable.Contains(ident)
		|| GlobalStructTable.Contains(ident)
		|| GlobalTraitTable.Contains(ident)
		|| GlobalFunctionTable.Contains(ident)
		|| (ident.Module.Name is "" && LocalVariablesManager.IsInScope(ident.Name));

	public void EnsureSymbolNameIsUnique(SymbolIdent ident)
	{
		if (SymbolExists(ident))
		{
			throw new InvalidOperationException($"Symbol {ident} already exists");
		}
	}
}

public sealed class LocalVariablesManager(SymbolTableManager manager)
{
	private readonly SymbolTableManager _manager = manager;

	private Stack<LocalVariableTable> Tables { get; } = [];
	private LocalVariableTable CurrentScope => Tables.Peek();

	public void PushScope()
	{
		Tables.Push(new(_manager));
	}

	public void PopScope()
	{
		Tables.Pop();
	}

	public void EnsureNoScope()
	{
		if (Tables.Count != 0)
			throw new InvalidOperationException($"Scope was {Tables.Count}");
	}

	public void AddLocal(LocalVariableSymbol variable)
	{
		_manager.EnsureSymbolNameIsUnique(variable.Ident.AsGlobal());
		CurrentScope.Add(variable);
	}

	public bool IsInScope(string name)
	{
		var ident = IdentHelper.ForLocalVariable(name);
		// foreach over a stack enumerates from top to bottom
		foreach (var table in Tables)
		{
			if (table.Contains(ident))
				return true;
		}

		return false;
	}

	public bool TryGet(string name, [NotNullWhen(true)] out LocalVariableSymbol? symbol)
	{
		var ident = IdentHelper.ForLocalVariable(name);
		foreach (var table in Tables)
		{
			if (table.TryGet(ident, out symbol))
			{
				return true;
			}
		}

		symbol = null;
		return false;
	}
}
