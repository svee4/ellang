using Ellang.Compiler.Lexing;
using Ellang.Compiler.Parsing.Nodes;

namespace Ellang.Compiler.Parsing.Parselets;

public sealed class DiscardExpressionParselet : ParseletBase, IPrefixParselet
{
	public IExpression Parse(Parser parser)
	{
		using var scope = Scope(parser);

		_ = parser.Eat<UnderscoreKeyword>();
		_ = parser.Eat<Equal>();

		var expr = parser.ParseExpression();

		_ = parser.EatIf<SemiColon>();

		return expr;
	}
}
