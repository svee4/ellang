using Ellang.Compiler.Lexing;
using Ellang.Compiler.Parsing.Nodes;

namespace Ellang.Compiler.Parsing.Parselets;

public sealed class GroupingParselet : ParseletBase, IPrefixParselet
{
	public IExpression Parse(Parser parser)
	{
		using var scope = Scope(parser);

		_ = parser.Eat<OpenBrace>();
		var expr = parser.ParseSubExpression(Precedence.None);
		_ = parser.Eat<CloseBrace>();

		return expr;
	}
}
