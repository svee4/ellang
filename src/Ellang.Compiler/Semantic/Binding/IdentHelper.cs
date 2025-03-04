using Ellang.Compiler.Parser.Nodes;

namespace Ellang.Compiler.AST.Binding;


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
		SymbolIdent.From(FormatArity(st.Name.Value, st.TypeParameters.Nodes.Count), module);

	public SymbolIdent ForStructCurComp(StructDeclarationStatement st) =>
		ForStruct(st, CurComp);

	public static SymbolIdent ForFunc(FunctionDeclarationStatement st, string module) =>
		SymbolIdent.From(FormatArity(st.Name.Value, st.TypeParameters.Nodes.Count), module);

	public SymbolIdent ForFuncCurComp(FunctionDeclarationStatement st) =>
		ForFunc(st, CurComp);

	public SymbolIdent ForTypeRef(TypeRef type) =>
		SymbolIdent.From(FormatArity(type.Identifier.Value, type.Generics.Count), type.Identifier.Module ?? CurComp);

	public SymbolIdent FromIdentifier(Identifier ident) =>
		ident.Module is { } mod ? SymbolIdent.From(ident.Value, mod) : ForCurComp(ident.Value);
}
