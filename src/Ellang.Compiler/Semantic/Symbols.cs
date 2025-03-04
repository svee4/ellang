using Ellang.Compiler.Parser.Nodes;

namespace Ellang.Compiler.AST;

public interface ISymbol;
public interface INamedTypeSymbol
{
	SymbolIdent Ident { get; }
}

public sealed record ModuleSymbol(string Name);
public sealed record SymbolIdent(string Name, ModuleSymbol Module)
{
	public override string ToString() =>
		$"{Module.Name}::{Name}";

	public static SymbolIdent From(string symbolName, string moduleName) =>
		new SymbolIdent(symbolName, new ModuleSymbol(moduleName));

	public static SymbolIdent CoreLib(string symbol) =>
		From(symbol, Constants.CoreLibModuleName);
}

public abstract record GlobalSymbol(SymbolIdent Ident);

public sealed record VariableSymbol(SymbolIdent Ident, TypeReferenceSymbol Type) : GlobalSymbol(Ident), ISymbol;
public sealed record StructSymbol(
	SymbolIdent Ident,
	List<TypeParameterSymbol> GenericParameters,
	List<StructFieldSymbol> Fields,
	StructDeclarationStatement? Syntax) : GlobalSymbol(Ident), INamedTypeSymbol;

public sealed record TraitSymbol(SymbolIdent Ident, List<SymbolIdent> TypeParameters,
	StructDeclarationStatement? Syntax) : GlobalSymbol(Ident), INamedTypeSymbol;

public interface IFunctionSymbol : ISymbol
{
	TypeReferenceSymbol ReturnType { get; }
	List<TypeParameterSymbol> TypeParameters { get; }
	List<FunctionParameterSymbol> Parameters { get; }
	List<IOperation> Operations { get; }
}

public sealed record UnnamedFunctionSymbol(
	TypeReferenceSymbol ReturnType,
	List<TypeParameterSymbol> TypeParameters,
	List<FunctionParameterSymbol> Parameters,
	List<IOperation> Operations,
	FunctionDeclarationStatement? Syntax) : IFunctionSymbol;

public sealed record NamedFunctionSymbol(
	SymbolIdent Ident,
	TypeReferenceSymbol ReturnType,
	List<TypeParameterSymbol> TypeParameters,
	List<FunctionParameterSymbol> Parameters,
	List<IOperation> Operations,
	FunctionDeclarationStatement? Syntax) : GlobalSymbol(Ident), IFunctionSymbol;


public abstract record TypeReferenceSymbol(SymbolIdent Ident);
public abstract record NamedTypeReferenceSymbol(INamedTypeSymbol Symbol) : TypeReferenceSymbol(Symbol.Ident);

public sealed record StructTypeReferenceSymbol(StructSymbol Source) : NamedTypeReferenceSymbol(Source);
public sealed record TraitReferenceTypeSymbol(TraitSymbol Source) : NamedTypeReferenceSymbol(Source);

public sealed record GenericTypeReferenceSymbol(TypeReferenceSymbol Type, List<TypeReferenceSymbol> GenericArguments)
	: TypeReferenceSymbol(Type.Ident);

public sealed record PointerTypeReferenceSymbol(TypeReferenceSymbol Type, int PointerCount) : TypeReferenceSymbol(Type.Ident);

public sealed record TypeParameterReferenceSymbol(TypeParameterSymbol Source)
	: TypeReferenceSymbol(SymbolIdent.From(Source.Name, ""));

public sealed record UnboundTypeReferenceSymbol(SymbolIdent Ident) : TypeReferenceSymbol(Ident);


public sealed record FunctionParameterSymbol(string Name, TypeReferenceSymbol Type);

public sealed record TypeParameterSymbol(string Name);

public sealed record StructFieldSymbol(string Name, TypeReferenceSymbol Type);

