using Ellang.Compiler.Parser.Nodes;
using Ellang.Compiler.Semantic;

namespace Ellang.Compiler.Semantic.Binding;


public sealed class IdentHelper(Binder analyzer)
{
	private readonly Binder _analyzer = analyzer;

	private string CurComp => _analyzer.Compilation.ModuleName;

	private static string FormatArity(string name, int arity) =>
		arity > 0 ? $"{name}`{arity}" : name;

	public static SymbolIdent ForLocalVariable(string localName) =>
		SymbolIdent.From(localName, "");

	public SymbolIdent ForCurComp(string symbol) =>
		SymbolIdent.From(symbol, CurComp);

	public static SymbolIdent ForStruct(StructDeclarationStatement st, string module) =>
		SymbolIdent.From(FormatArity(st.Name.Value, st.TypeParameters.Count), module);

	public SymbolIdent ForStructCurComp(StructDeclarationStatement st) =>
		ForStruct(st, CurComp);

	public static SymbolIdent ForNamedFunc(FunctionDeclarationStatement st, string module) =>
		SymbolIdent.From(FormatArity(st.Name.Value, st.TypeParameters.Count), module);

	public SymbolIdent ForNamedFuncCurComp(FunctionDeclarationStatement st) =>
		ForNamedFunc(st, CurComp);

	public SymbolIdent ForAnonFunc() => throw new NotImplementedException();

	public SymbolIdent ForTypeRef(TypeRef type) =>
		SymbolIdent.From(FormatArity(type.Identifier.Value, type.Generics.Count), type.Identifier.Module ?? CurComp);

	public SymbolIdent FromIdentifier(Identifier ident) =>
		ident.Module is { } mod ? SymbolIdent.From(ident.Value, mod) : ForCurComp(ident.Value);
}
