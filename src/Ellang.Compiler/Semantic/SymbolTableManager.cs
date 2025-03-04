using Ellang.Compiler.AST.Binding;
using System.Diagnostics.CodeAnalysis;

namespace Ellang.Compiler.AST;

public sealed class SymbolTableManager
{
	public VariableTable GlobalVariableTable { get; private set; }
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
	private SymbolTableManager _manager = manager;

	private Stack<VariableTable> LocalVariables { get; } = [];
	private VariableTable CurrentScope => LocalVariables.Peek();

	public void PushScope()
	{
		LocalVariables.Push(new(_manager));
	}

	public void PopScope()
	{
		LocalVariables.Pop();
	}

	public void EnsureNoScope()
	{
		if (LocalVariables.Count != 0)
		{
			throw new InvalidOperationException($"Scope was {LocalVariables.Count}");
		}
	}

	public void AddLocal(VariableSymbol variable)
	{
		_manager.EnsureSymbolNameIsUnique(variable.Ident);
		CurrentScope.Add(variable);
	}

	public bool IsInScope(string name)
	{
		var ident = IdentHelper.ForLocalVariable(name);
		// foreach over a stack enumerates from top to bottom
		foreach (var table in LocalVariables)
		{
			if (table.Contains(ident))
			{
				return true;
			}
		}

		return false;
	}

	public bool TryGet(string name, [NotNullWhen(true)] out VariableSymbol? symbol)
	{
		var ident = IdentHelper.ForLocalVariable(name);
		foreach (var table in LocalVariables)
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
