using Ellang.Compiler.Lexer;
using Ellang.Compiler.Parser.Nodes;

namespace Ellang.Compiler.Parser.Parselets;

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
