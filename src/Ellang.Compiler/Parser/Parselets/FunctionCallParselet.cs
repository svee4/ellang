using Ellang.Compiler.Lexer;
using Ellang.Compiler.Parser.Nodes;

namespace Ellang.Compiler.Parser.Parselets;

// a function does not need to be called with its name:
// var x: int = (func _(): int { return 3; })(); should be valid (in the future when function expression are implemented),
// as well as GetHof()(); where GetHof is func GetHof(): TODO: how to express function types?
public sealed class FunctionCallParselet : ParseletBase, IInfixParselet
{
	public Precedence Precedence => Precedence.FunctionCall;

	public IExpression Parse(Parser parser, IExpression left)
	{
		using var scope = Scope(parser);

		_ = parser.Eat<OpenParen>();
		List<FunctionArgument> arguments = [];

		while (parser.Peek() is not CloseParen)
		{
			arguments.Add(new FunctionArgument(parser.ParseSubExpression(Precedence.None)));
			_ = parser.EatIf<Comma>();
		}

		// single trailing commas in parameter list are allowed

		_ = parser.Eat<CloseParen>();
		return new FunctionCallExpression(left, arguments);
	}
}
