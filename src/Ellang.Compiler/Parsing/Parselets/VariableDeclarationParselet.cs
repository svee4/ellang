using Ellang.Compiler.Lexing;
using Ellang.Compiler.Parsing.Nodes;

namespace Ellang.Compiler.Parsing.Parselets;

public sealed class VariableDeclarationParselet : ParseletBase, IPrefixParselet
{
	public IExpression Parse(Parser parser)
	{
		using var scope = Scope(parser);
		var expr = parser.ParseVariableDeclaration();
		_ = parser.EatIf<SemiColon>();
		return expr;
	}
}
