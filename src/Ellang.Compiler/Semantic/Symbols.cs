using Ellang.Compiler.Parser.Nodes;

namespace Ellang.Compiler.Semantic;

public interface ISymbol;

public interface INamedSymbol
{
	SymbolIdent Ident { get; }
}

public interface IMemberSymbol : INamedSymbol
{
	TypeReferenceSymbol Type { get; }
	LocalSymbolIdent LocalIdent => ((INamedSymbol)this).Ident.AsLocal();
}

public sealed record ModuleSymbol(string Name);

/// <summary>
/// A symbol identifier that contains a module. e.g. a function, struct or trait from this or any other compilation
/// </summary>
public sealed record SymbolIdent(string Name, ModuleSymbol Module)
{
	public override string ToString() =>
		$"{Module.Name}::{Name}";

	public static SymbolIdent From(string symbolName, string moduleName) =>
		new SymbolIdent(symbolName, new ModuleSymbol(moduleName));

	public static SymbolIdent CoreLib(string symbol) =>
		From(symbol, Constants.CoreLibModuleName);

	public LocalSymbolIdent AsLocal() => new LocalSymbolIdent(Name);
}

/// <summary>
/// A symbol identifier without a module. e.g. type parameter, parameter, local variable, struct field
/// </summary>
public sealed record LocalSymbolIdent(string Name)
{
	public override string ToString() => $"::{Name}";

	public SymbolIdent AsGlobal() => new SymbolIdent(Name, new ModuleSymbol(""));
}

public abstract record GlobalSymbol(SymbolIdent Ident);

public sealed record GlobalVariableSymbol(SymbolIdent Ident, TypeReferenceSymbol Type) 
	: GlobalSymbol(Ident), INamedSymbol;

public sealed record StructSymbol(
	SymbolIdent Ident,
	List<TypeParameterSymbol> GenericParameters,
	List<StructFieldSymbol> Fields,
	List<StructMethodSymbol> Methods,
	StructDeclarationStatement? Syntax) : GlobalSymbol(Ident), INamedSymbol;

public sealed record TraitSymbol(
	SymbolIdent Ident,
	List<SymbolIdent> TypeParameters,
	StructDeclarationStatement? Syntax) : GlobalSymbol(Ident), INamedSymbol;

public interface IFunctionSymbol : ISymbol
{
	SymbolIdent Ident { get; }
	TypeReferenceSymbol ReturnType { get; }
	List<TypeParameterSymbol> TypeParameters { get; }
	List<FunctionParameterSymbol> Parameters { get; }
	List<IOperation> Operations { get; }
}

/// <summary>
/// A function that does not have an identifier. e.g. a function expression
/// </summary>
public sealed record UnnamedFunctionSymbol(
	SymbolIdent Ident,
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
	FunctionDeclarationStatement? Syntax) : GlobalSymbol(Ident), IFunctionSymbol, INamedSymbol;

public sealed record StructMethodSymbol(
	NamedFunctionSymbol Function,
	StructMethodDeclarationStatement? Syntax) : IFunctionSymbol, IMemberSymbol
{
	public LocalSymbolIdent Ident => Function.Ident.AsLocal();
	public TypeReferenceSymbol Type { get; } = new FunctionTypeReferenceSymbol(Function);

	SymbolIdent IFunctionSymbol.Ident => Function.Ident;
	public TypeReferenceSymbol ReturnType => Function.ReturnType;
	public List<TypeParameterSymbol> TypeParameters => Function.TypeParameters;
	public List<FunctionParameterSymbol> Parameters => Function.Parameters;
	public List<IOperation> Operations => Function.Operations;

	SymbolIdent INamedSymbol.Ident => Function.Ident;
}

public abstract record TypeReferenceSymbol(SymbolIdent Ident);
public abstract record NamedTypeReferenceSymbol(INamedSymbol Symbol) : TypeReferenceSymbol(Symbol.Ident);

public sealed record StructTypeReferenceSymbol(StructSymbol Source) : NamedTypeReferenceSymbol(Source);
public sealed record TraitReferenceTypeSymbol(TraitSymbol Source) : NamedTypeReferenceSymbol(Source);

public sealed record FunctionTypeReferenceSymbol(IFunctionSymbol Source) : TypeReferenceSymbol(Source.Ident);

public sealed record GenericTypeReferenceSymbol(TypeReferenceSymbol UnderlyingType, List<TypeReferenceSymbol> GenericArguments)
	: TypeReferenceSymbol(UnderlyingType.Ident);

public sealed record PointerTypeReferenceSymbol(TypeReferenceSymbol UnderlyingType, int PointerCount) 
	: TypeReferenceSymbol(UnderlyingType.Ident);

public sealed record TypeParameterTypeReferenceSymbol(TypeParameterSymbol Source)
	: TypeReferenceSymbol(SymbolIdent.From(Source.Ident.Name, ""));

public sealed record UnboundTypeReferenceSymbol(SymbolIdent Ident) : TypeReferenceSymbol(Ident);


public sealed record LocalVariableSymbol(LocalSymbolIdent Ident, TypeReferenceSymbol Type) : INamedSymbol
{
	SymbolIdent INamedSymbol.Ident => Ident.AsGlobal();
}

public sealed record FunctionParameterSymbol(LocalSymbolIdent Ident, TypeReferenceSymbol Type);
public sealed record TypeParameterSymbol(LocalSymbolIdent Ident);
public sealed record StructFieldSymbol(LocalSymbolIdent Ident, TypeReferenceSymbol Type) : IMemberSymbol
{
	SymbolIdent INamedSymbol.Ident => Ident.AsGlobal();
}
