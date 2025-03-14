using Ellang.Compiler.Lexing;
using Ellang.Compiler.Parsing.Nodes;
using System.Diagnostics;

namespace Ellang.Compiler.Parsing.Parselets;

public sealed class MemberAccessParselet : ParseletBase, IInfixParselet
{
	public Precedence GetPrecedence() => Precedence.Primary;

	public IExpression Parse(Parser parser, IExpression left)
	{
		using var scope = Scope(parser);

		_ = parser.Eat<Dot>();
		var token = parser.Peek<IdentifierLiteral>();

		var expr = parser.ParseSubExpression(Precedence.Primary);
		if (expr is not IdentifierExpression idexpr)
		{
			parser.Throw(token.Line, token.Column, "Expected {Expected}, got {Actual}",
				typeof(IdentifierExpression),
				expr.GetType());

			throw new UnreachableException();
		}

		return new MemberAccessExpression(left, idexpr);
	}
}
