using System.Diagnostics.CodeAnalysis;
using Ellang.Compiler.Compilation;
using Ellang.Compiler.Parser;
using Ellang.Compiler.Parser.Nodes;

namespace Ellang.Compiler.AST.Binding;

public sealed class Binder
{
	public AFcukingCompilation Compilation { get; }
	public SymbolTableManager SymbolManager { get; } = new();
	public IdentHelper IdentHelper { get; }

	public Binder(AFcukingCompilation compilation)
	{
		Compilation = compilation;
		IdentHelper = new IdentHelper(this);
	}

	public void Bind(SyntaxTree tree)
	{
		CreateSymbols(tree);
		BindSymbols(tree);
	}

	private void CreateSymbols(SyntaxTree tree)
	{
		foreach (var statement in tree.TopLevelStatements)
		{
			switch (statement)
			{
				case FunctionDeclarationStatement st:
				{
					CreateFunctionExpressionSymbols(st);
					break;
				}
				case StructDeclarationStatement st:
				{
					CreateStructDeclarationSymbols(st);
					break;
				}
				default: throw new InvalidOperationException($"Unhandled statement {statement.GetType()}");
			}
		}
	}

	private void CreateFunctionExpressionSymbols(FunctionDeclarationStatement st)
	{
		var name = st.Name.Value;
		var syntax = st;

		var returnType = BindTypeSyntaxToTypeSymbolOrGetUnbound(st.ReturnType);

		var parameters = st.Parameters.Nodes
			.Select(p => new FunctionParameterSymbol(p.Name.Value, BindTypeSyntaxToTypeSymbolOrGetUnbound(p.Type)))
			.ToList();

		var symbol = new NamedFunctionSymbol(IdentHelper.ForFuncCurComp(st), returnType, [], parameters, [], st);

		SymbolManager.GlobalFunctionTable.Add(symbol);
	}

	private void CreateStructDeclarationSymbols(StructDeclarationStatement st)
	{
		var fields = st.Fields.Nodes
			.Select(f => new StructFieldSymbol(f.Name.Value, BindTypeSyntaxToTypeSymbolOrGetUnbound(f.Type)))
			.ToList();

		var symbol = new StructSymbol(IdentHelper.ForStructCurComp(st), [], fields, st);
		SymbolManager.GlobalStructTable.Add(symbol);
	}

	private void BindSymbols(SyntaxTree tree)
	{
		foreach (var statement in tree.TopLevelStatements)
		{
			switch (statement)
			{
				case FunctionDeclarationStatement st:
				{
					BindFunctionExpressionSymbols(st);
					break;
				}
				case StructDeclarationStatement st:
				{
					BindStructDeclarationSymbols(st);
					break;
				}
				default: throw new InvalidOperationException($"Unhandled statement {statement.GetType()}");
			}
		}
	}

	private void BindFunctionExpressionSymbols(FunctionDeclarationStatement func)
	{
		SymbolManager.GlobalFunctionTable.Update(
			IdentHelper.ForFuncCurComp(func),
			prev => prev with
			{
				Parameters = prev.Parameters.Select(p => p with { Type = BindUnboundTypeSymbol(p.Type) }).ToList(),
				ReturnType = BindUnboundTypeSymbol(prev.ReturnType),
			});

		List<IOperation> operations = [];

		var parser = new OperationParser(this);

		SymbolManager.LocalVariablesManager.EnsureNoScope();
		SymbolManager.LocalVariablesManager.PushScope();

		foreach (var statement in func.Statements.Nodes)
		{
			switch (statement)
			{
				case FunctionCallExpression call:
				{
					operations.Add(parser.ParseExpression(call));
					break;
				}
				case AssignmentExpression ass:
				{
					operations.Add(parser.ParseExpression(ass));
					break;
				}
				case VariableDeclarationStatement decl:
				{
					var ident = IdentHelper.ForLocalVariable(decl.Name.Value);
					var type = BindUnboundTypeSymbol(BindTypeSyntaxToTypeSymbolOrGetUnbound(decl.Type));
					var sym = new VariableSymbol(ident, type);

					var init = parser.ParseExpression(decl.Initializer);
					var op = new VariableDeclarationOperation(sym, init);
					operations.Add(op);

					break;
				}
				case DiscardStatement st:
				{
					var op = parser.ParseExpression(st.Expression);
					operations.Add(op);

					break;
				}
				case EmptyStatement: break;

				default: throw new InvalidOperationException($"Unexpected statement {statement}");
			}
		}

		SymbolManager.GlobalFunctionTable.Update(IdentHelper.ForFuncCurComp(func),
			prev => prev with
			{
				Operations = operations,
			});

		SymbolManager.LocalVariablesManager.PopScope();
	}

	private void BindStructDeclarationSymbols(StructDeclarationStatement declaration)
	{
		List<StructFieldSymbol> fields = [];

		foreach (var field in declaration.Fields.Nodes)
		{
			var typeReferenceSymbol = BindUnboundTypeSymbol(BindTypeSyntaxToTypeSymbolOrGetUnbound(field.Type));
			var fieldSymbol = new StructFieldSymbol(field.Name.Value, typeReferenceSymbol);
			fields.Add(fieldSymbol);
		}

		SymbolManager.GlobalStructTable.Update(IdentHelper.ForStructCurComp(declaration),
			prev => prev with
			{
				Fields = fields
			});
	}

	private TypeReferenceSymbol BindUnboundTypeSymbol(TypeReferenceSymbol possiblyUnboundSymbol)
	{
		TypeReferenceSymbol bound;
		if (possiblyUnboundSymbol is UnboundTypeReferenceSymbol unbound)
		{
			if (!SymbolManager.GlobalStructTable.TryGet(unbound.Ident, out var target))
				throw new CouldNotBindTypeException(unbound.Ident.ToString());

			bound = new StructTypeReferenceSymbol(target);
		}
		else
		{
			bound = possiblyUnboundSymbol;
		}

		return bound;
	}

	private TypeReferenceSymbol BindTypeSyntaxToTypeSymbolOrGetUnbound(TypeRef type)
	{
		if (type is KeywordTypeRef kw)
		{
			return new StructTypeReferenceSymbol(
				SymbolManager.GlobalStructTable.Get(
					SymbolIdent.CoreLib(kw.CoreLibType)));
		}

		return MakeTypeSymbol(type, this);

		static TypeReferenceSymbol MakeTypeSymbol(TypeRef type, Binder binder)
		{
			TypeReferenceSymbol baseSymbol;

			var ident = binder.IdentHelper.ForTypeRef(type);

			baseSymbol = binder.SymbolManager.GlobalStructTable.TryGet(ident, out var structSymbol)
				? new StructTypeReferenceSymbol(structSymbol)
				: new UnboundTypeReferenceSymbol(binder.IdentHelper.ForTypeRef(type));

			if (type.Generics.Count > 0)
			{
				var generics = type.Generics.Select(t => MakeTypeSymbol(t, binder)).ToList();
				baseSymbol = new GenericTypeReferenceSymbol(baseSymbol, generics);
			}

			if (type.PointerCount > 0)
				baseSymbol = new PointerTypeReferenceSymbol(baseSymbol, type.PointerCount);

			return baseSymbol;
		}
	}

	[SuppressMessage("Design", "CA1032:Implement standard exception constructors", 
		Justification = "I dont want to")]
	public abstract class AnalyzerException(string message) : Exception(message);

	[SuppressMessage("Design", "CA1032:Implement standard exception constructors",
		Justification = "I dont want to")]
	public sealed class CouldNotBindTypeException : AnalyzerException
	{
		public string TypeName { get; }

		public CouldNotBindTypeException(string typeName) : base($"Could not bind type {typeName}")
		{
			TypeName = typeName;
		}

		public CouldNotBindTypeException(SymbolIdent ident) : this(ident.ToString())
		{
			TypeName = ident.ToString();
		}
	}
}
