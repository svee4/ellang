using Ellang.Compiler.Lexing;
using Ellang.Compiler.Parsing.Nodes;

namespace Ellang.Compiler.Parsing.Parselets;

public sealed class YieldExpressionParselet : ParseletBase, IPrefixParselet
{
	public IExpression Parse(Parser parser)
	{
		using var scope = Scope(parser);

		_ = parser.Eat<YieldKeyword>();

		IExpression? expr = parser.Peek() is SemiColon
			? null
			: parser.ParseExpression();

		_ = parser.Eat<SemiColon>();

		return new YieldExpression(expr);
	}
}
