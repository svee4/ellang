using Ellang.Compiler.Lexer;
using Ellang.Compiler.Parser.Nodes;
using System.Diagnostics;

namespace Ellang.Compiler.Parser.Parselets;

// same as BinaryOperatorParselet, this parselet is used for all prefixed unary operators
// as listed in the switch
public sealed class PrefixOperatorParselet<TToken> : ParseletBase, IPrefixParselet where TToken : LexerToken
{
	public PrefixOperatorParselet() =>
		Debug.Assert(PrefixOperatorParselet.SupportedTokens.Contains(typeof(TToken)));

	public IExpression Parse(Parser parser)
	{
		using var scope = Scope(parser);

		var token = parser.Eat<TToken>();
		var expr = parser.ParseSubExpression(Precedence.Prefix);

		return token switch
		{
			Bang => new LogicalNegationExpression(expr),
			Minus => new MathematicalNegationExpression(expr),
			Star => new DereferenceExpression(expr),
			_ => parser.Throw<IExpression>("Unimplemented or unsupported unary prefix operator {Token}", token)
		};
	}
}

public static class PrefixOperatorParselet
{
	public static IReadOnlyList<Type> SupportedTokens => [typeof(Bang), typeof(Minus), typeof(Star)];
}
