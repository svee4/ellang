using Ellang.Compiler.Lexer;
using Ellang.Compiler.Parser.Nodes;

namespace Ellang.Compiler.Parser.Parselets;

public sealed class IndexerCallParselet : ParseletBase, IInfixParselet
{
	public Precedence GetPrecedence() => Precedence.Primary;

	public IExpression Parse(Parser parser, IExpression left)
	{
		using var scope = Scope(parser);

		_ = parser.Eat<OpenBracket>();
		var indexerExpr = parser.ParseSubExpression(Precedence.None);
		_ = parser.Eat<CloseBracket>();

		return new IndexerCallExpression(left, indexerExpr);
	}
}
