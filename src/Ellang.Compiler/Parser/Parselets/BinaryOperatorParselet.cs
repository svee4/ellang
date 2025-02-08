using Ellang.Compiler.Lexer;
using Ellang.Compiler.Parser.Nodes;
using System.Diagnostics;

namespace Ellang.Compiler.Parser.Parselets;

// EVERYTHING IS LEFT ASSOCIATIVE
// this class is used for all binary operators as listed in the switch
public sealed class BinaryOperatorParselet<TToken> : ParseletBase, IInfixParselet where TToken : LexerToken
{
	public Precedence Precedence { get; }

	public BinaryOperatorParselet(Precedence precedence)
	{
		Debug.Assert(BinaryOperatorParselet.SupportedTokens.Contains(typeof(TToken)));
		Precedence = precedence;
	}

	public IExpression Parse(Parser parser, IExpression left)
	{
		using var scope = Scope(parser);

		var token = parser.Eat<TToken>();
		var expr = parser.ParseSubExpression(Precedence);

		return token switch
		{
			Plus => new AdditionExpression(left, expr),
			Minus => new SubtractionExpression(left, expr),
			Star => new MultiplicationExpression(left, expr),
			Slash => new DivisionExpression(left, expr),
			_ => parser.Throw<IExpression>("Unimplemented or unsupported binary operator {Token}", token)
		};
	}
}

public static class BinaryOperatorParselet
{
	public static IReadOnlyList<Type> SupportedTokens => [typeof(Plus), typeof(Minus), typeof(Star), typeof(Slash)];
}
