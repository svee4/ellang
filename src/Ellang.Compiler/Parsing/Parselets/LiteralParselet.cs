using Ellang.Compiler.Lexing;
using Ellang.Compiler.Parsing.Nodes;

namespace Ellang.Compiler.Parsing.Parselets;

public sealed class LiteralParselet<TToken> : ParseletBase, IPrefixParselet where TToken : LexerToken
{
	public IExpression Parse(Parser parser)
	{
		using var scope = Scope(parser);
		return parser.Eat<TToken>() switch
		{
			StringLiteral s => new StringLiteralExpression(s.Value),
			NumericLiteral n => new IntLiteralExpression(n.Value),
			var token => parser.ThrowAt<IExpression>(token, "Invalid literal {Token}", token)
		};
	}
}
