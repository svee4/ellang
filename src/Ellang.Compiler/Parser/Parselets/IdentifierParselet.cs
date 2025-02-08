using Ellang.Compiler.Lexer;
using Ellang.Compiler.Parser.Nodes;

namespace Ellang.Compiler.Parser.Parselets;

public sealed class IdentifierParselet : ParseletBase, IPrefixParselet
{
	public IExpression Parse(Parser parser)
	{
		using var scope = Scope(parser);
		var value = parser.Eat<IdentifierLiteral>();
		return new IdentifierExpression(new Identifier(value.Value));
	}
}
