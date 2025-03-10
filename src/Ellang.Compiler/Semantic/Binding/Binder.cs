using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Ellang.Compiler.Parser;
using Ellang.Compiler.Parser.Nodes;
using Microsoft.Extensions.Logging;

namespace Ellang.Compiler.Semantic.Binding;

public sealed class Binder
{
	public ILogger<Binder> Logger { get; }
	public AFcukingCompilation Compilation { get; }
	public SymbolTableManager SymbolManager { get; } = new();
	public IdentHelper IdentHelper { get; }

	public Binder(AFcukingCompilation compilation, ILogger<Binder> logger)
	{
		Compilation = compilation;
		Logger = logger;
		IdentHelper = new IdentHelper(this);
	}

	public void Bind(SyntaxTree tree)
	{
		CreateSymbols(tree);
		BindSymbols(tree);
	}

	private void CreateSymbols(SyntaxTree tree)
	{
		using var scope = Logger.BeginScope(nameof(CreateSymbols));

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
		using var scope = Logger.BeginScope(nameof(CreateFunctionExpressionSymbols));

		var name = st.Name.Value;
		var syntax = st;

		var returnType = BindTypeSyntaxToTypeSymbolOrGetUnbound(st.ReturnType);

		var parameters = st.Parameters.Nodes
			.Select(p => new FunctionParameterSymbol(
				new LocalSymbolIdent(p.Name.Value),
				BindTypeSyntaxToTypeSymbolOrGetUnbound(p.Type)))
			.ToList();

		var symbol = new NamedFunctionSymbol(IdentHelper.ForNamedFuncCurComp(st), returnType, [], parameters, [], st);

		Logger.LogDebug("Created function {Name}({Parameters}): {ReturnType}",
			symbol.Ident,
			string.Join(", ", parameters.Select(p => $"{p.Ident}: {p.Type.Ident}")),
			returnType.Ident);

		SymbolManager.GlobalFunctionTable.Add(symbol);
	}

	private void CreateStructDeclarationSymbols(StructDeclarationStatement st)
	{
		using var scope = Logger.BeginScope(nameof(CreateStructDeclarationSymbols));

		var fields = st.Fields.Nodes
			.Select(f => new StructFieldSymbol(
				new LocalSymbolIdent(f.Name.Value),
				BindTypeSyntaxToTypeSymbolOrGetUnbound(f.Type)))
			.ToList();

		var symbol = new StructSymbol(IdentHelper.ForStructCurComp(st), [], fields, [], st);

		Logger.LogDebug("Created struct {Name} {{ {Fields} }} ",
			symbol.Ident,
			string.Join("; ", fields.Select(f => $"{f.Ident}: {f.Type.Ident}")));

		SymbolManager.GlobalStructTable.Add(symbol);
	}

	private void BindSymbols(SyntaxTree tree)
	{
		using var scope = Logger.BeginScope(nameof(BindSymbols));

		foreach (var statement in tree.TopLevelStatements)
		{
			switch (statement)
			{
				case FunctionDeclarationStatement st:
				{
					BindFunctionStatementSymbols(st);
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

	private void BindFunctionStatementSymbols(FunctionDeclarationStatement func)
	{
		var funcIdent = IdentHelper.ForNamedFuncCurComp(func);

		using var scope = Logger.BeginScope(nameof(BindFunctionStatementSymbols) + " - {FunctionIdentifier}", funcIdent);

		SymbolManager.GlobalFunctionTable.Update(
			funcIdent,
			prev => prev with
			{
				Parameters = prev.Parameters.Select(p => p with { Type = BindUnboundTypeSymbol(p.Type) }).ToList(),
				ReturnType = BindUnboundTypeSymbol(prev.ReturnType),
			});

		var parser = new OperationParser(this);
		List<IOperation> operations = [];

		SymbolManager.LocalVariablesManager.EnsureNoScope();
		SymbolManager.LocalVariablesManager.PushScope();

		using (Logger.BeginScope("[parameters]"))
		{
			var symbol = SymbolManager.GlobalFunctionTable.Get(funcIdent);
			foreach (var parameter in symbol.Parameters)
			{
				var ident = parameter.Ident;
				var type = parameter.Type;
				SymbolManager.LocalVariablesManager.AddLocal(new LocalVariableSymbol(ident, type));
			}
		}

		using (Logger.BeginScope("[operations]"))
		{
			foreach (var statement in func.Statements.Nodes)
			{
				IOperation op;
				switch (statement)
				{
					case FunctionCallExpression call:
					{
						op = parser.ParseExpression(call);
						break;
					}
					case AssignmentExpression ass:
					{
						op = parser.ParseExpression(ass);
						break;
					}
					case VariableDeclarationStatement decl:
					{
						var ident = IdentHelper.ForLocalVariable(decl.Name.Value);
						var type = BindUnboundTypeSymbol(BindTypeSyntaxToTypeSymbolOrGetUnbound(decl.Type));
						var sym = new GlobalVariableSymbol(ident, type);

						var init = parser.ParseExpression(decl.Initializer);
						op = new VariableDeclarationOperation(sym, init);

						var local = new LocalVariableSymbol(new LocalSymbolIdent(ident.Name), type);
						SymbolManager.LocalVariablesManager.AddLocal(local);

						break;
					}
					case DiscardStatement st:
					{
						op = parser.ParseExpression(st.Expression);
						break;
					}
					case EmptyStatement: continue;
					default: throw new InvalidOperationException($"Unexpected statement {statement}");
				}

				Logger.LogDebug("Added operation {Operation}", op);
				operations.Add(op);
			}
		}

		SymbolManager.GlobalFunctionTable.Update(IdentHelper.ForNamedFuncCurComp(func),
			prev => prev with
			{
				Operations = operations,
			});

		SymbolManager.LocalVariablesManager.PopScope();
	}

	private void BindStructDeclarationSymbols(StructDeclarationStatement declaration)
	{
		using var scope = Logger.BeginScope(nameof(BindStructDeclarationSymbols));

		var structIdent = IdentHelper.ForStructCurComp(declaration);

		List<StructFieldSymbol> fields = [];

		using (Logger.BeginScope("[{StructIdentifier} fields]", structIdent))
		{
			foreach (var field in declaration.Fields.Nodes)
			{
				var typeReferenceSymbol = BindUnboundTypeSymbol(BindTypeSyntaxToTypeSymbolOrGetUnbound(field.Type));
				var fieldSymbol = new StructFieldSymbol(new LocalSymbolIdent(field.Name.Value), typeReferenceSymbol);
				fields.Add(fieldSymbol);
			}
		}

		SymbolManager.GlobalStructTable.Update(structIdent,
			prev => prev with
			{
				Fields = fields
			});
	}

	private TypeReferenceSymbol BindUnboundTypeSymbol(TypeReferenceSymbol possiblyUnboundSymbol)
	{
		using var scope = Logger.BeginScope(nameof(BindUnboundTypeSymbol));
		return RecursivelyBindSymbol(possiblyUnboundSymbol, this);

		static TypeReferenceSymbol RecursivelyBindSymbol(TypeReferenceSymbol symbol, Binder binder)
		{
			using var scope = binder.Logger.BeginScope("[{SymbolIdentifier}]", symbol.Ident);

			switch (symbol)
			{
				case UnboundTypeReferenceSymbol:
				{
					return binder.SymbolManager.GlobalStructTable.TryGet(symbol.Ident, out var structSymbol)
						? new StructTypeReferenceSymbol(structSymbol)
						: throw new CouldNotBindTypeException(symbol.Ident);
				}
				case StructTypeReferenceSymbol sref:
				{
					return sref;
				}
				case GenericTypeReferenceSymbol gen:
				{
					var underlying = RecursivelyBindSymbol(gen.UnderlyingType, binder);

					List<TypeReferenceSymbol> genericArgs = [];
					foreach (var arg in gen.GenericArguments)
					{
						genericArgs.Add(RecursivelyBindSymbol(arg, binder));
					}

					return new GenericTypeReferenceSymbol(underlying, genericArgs);
				}
				case PointerTypeReferenceSymbol ptr:
				{
					var underlying = RecursivelyBindSymbol(ptr.UnderlyingType, binder);
					return new PointerTypeReferenceSymbol(underlying, ptr.PointerCount);
				}

				case null: throw new ArgumentNullException(nameof(symbol));
				default: throw new UnreachableException($"Unhandled type reference kind {symbol.GetType()}");
			}
		}
	}

	private TypeReferenceSymbol BindTypeSyntaxToTypeSymbolOrGetUnbound(TypeRef type)
	{
		return MakeTypeSymbol(type, this);

		static TypeReferenceSymbol MakeTypeSymbol(TypeRef type, Binder binder)
		{
			TypeReferenceSymbol baseSymbol;

			var ident = binder.IdentHelper.ForTypeRef(type);

			baseSymbol = binder.SymbolManager.GlobalStructTable.TryGet(ident, out var structSymbol)
				? new StructTypeReferenceSymbol(structSymbol)
				: new UnboundTypeReferenceSymbol(ident);

			if (type.Generics.Count > 0)
			{
				var generics = type.Generics.Select(t => MakeTypeSymbol(t, binder)).ToList();
				baseSymbol = new GenericTypeReferenceSymbol(baseSymbol, generics);
			}

			if (type.PointerCount > 0)
			{
				baseSymbol = new PointerTypeReferenceSymbol(baseSymbol, type.PointerCount);
			}

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

		public CouldNotBindTypeException(SymbolIdent ident) : this(ident.ToString()) { }
	}
}
