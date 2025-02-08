using Ellang.Compiler.Lexer;
using Ellang.Compiler.Parser.Nodes;
using System.Diagnostics;

namespace Ellang.Compiler.Parser.Parselets;

public sealed class LiteralParselet<TToken> : ParseletBase, IPrefixParselet where TToken : LexerToken
{
	public LiteralParselet() =>
		Debug.Assert(LiteralParselet.SupportedTokens.Contains(typeof(TToken)));

	public IExpression Parse(Parser parser)
	{
		using var scope = Scope(parser);
		return parser.Eat() switch
		{
			StringLiteral s => new StringLiteralExpression(s.Value),
			NumericLiteral n => new IntLiteralExpression(n.Value),
			var token => parser.Throw<IExpression>("Invalid literal {Token}", token)
		};
	}
}

public static class LiteralParselet
{
	public static IReadOnlyList<Type> SupportedTokens => [typeof(StringLiteral), typeof(NumericLiteral)];
}
