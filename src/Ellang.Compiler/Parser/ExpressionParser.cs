using Ellang.Compiler.Lexer;
using Ellang.Compiler.Parser.Nodes;
using Ellang.Compiler.Parser.Parselets;
using System.Collections.Frozen;
using System.Diagnostics;

namespace Ellang.Compiler.Parser;

public sealed class ExpressionParser(Parser parser)
{
	private readonly Parser _parser = parser;

	public IExpression ParseExpression(Precedence precedence)
	{
		using var scope = _parser.Scope();

		var token = _parser.Peek();
		if (token is SemiColon)
		{
			_parser.Throw("Expected expression, got semicolon");
			throw new UnreachableException();
		}

		if (!PrefixParselets.TryGetValue(token.GetType(), out var prefixParselet))
		{
			_parser.Throw("No prefix parselet for token {Token}", token);
			throw new UnreachableException();
		}

		var expr = prefixParselet.Parse(_parser);

		while (InfixParselets.TryGetValue(_parser.Peek().GetType(), out var infixParselet) && precedence < infixParselet.Precedence)
		{
			expr = infixParselet.Parse(_parser, expr);
		}

		return expr;
	}

	private static readonly FrozenDictionary<Type, IPrefixParselet> PrefixParselets = new Dictionary<Type, IPrefixParselet>
	{
		{ typeof(OpenParen), new GroupingParselet() },
		{ typeof(Bang), new PrefixOperatorParselet<Bang>() },
		{ typeof(Minus), new PrefixOperatorParselet<Minus>() },
		{ typeof(Star), new PrefixOperatorParselet<Star>() },
		{ typeof(IdentifierLiteral), new IdentifierParselet() },
		{ typeof(StringLiteral), new LiteralParselet<StringLiteral>() },
		{ typeof(NumericLiteral), new LiteralParselet<NumericLiteral>() },
	}.ToFrozenDictionary();

	private static readonly FrozenDictionary<Type, IInfixParselet> InfixParselets = new Dictionary<Type, IInfixParselet>
	{
		{ typeof(Equal), new AssignmentParselet() },
		{ typeof(OpenParen), new FunctionCallParselet() },
		{ typeof(Plus), new BinaryOperatorParselet<Plus>(Precedence.AdditionOrSubtraction) },
		{ typeof(Minus), new BinaryOperatorParselet<Minus>(Precedence.AdditionOrSubtraction) },
		{ typeof(Star), new BinaryOperatorParselet<Star>(Precedence.MultiplicationOrDivision) },
		{ typeof(Slash), new BinaryOperatorParselet<Slash>(Precedence.MultiplicationOrDivision) },
	}.ToFrozenDictionary();
}
