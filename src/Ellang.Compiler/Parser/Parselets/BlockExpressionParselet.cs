using Ellang.Compiler.Lexer;
using Ellang.Compiler.Parser.Nodes;

namespace Ellang.Compiler.Parser.Parselets;

public sealed class BlockExpressionParselet : ParseletBase, IPrefixParselet
{
	public IExpression Parse(Parser parser)
	{
		using var scope = Scope(parser);
		_ = parser.Eat<OpenBrace>();

		List<IExpressionStatement> statements = [];
		while (parser.Peek() is not CloseBrace)
		{
			statements.Add(parser.ParseExpressionStatement());
		}

		_ = parser.Eat<CloseBrace>();
		return new BlockExpression(statements);
	}
}
