using Ellang.Compiler.Parsing.Nodes;

namespace Ellang.Compiler.Parsing.Parselets;

public sealed class FunctionExpressionParselet : ParseletBase, IPrefixParselet
{
	public IExpression Parse(Parser parser)
	{
		using var scope = Scope(parser);
		return parser.ParseFunctionExpression();
	}
}
