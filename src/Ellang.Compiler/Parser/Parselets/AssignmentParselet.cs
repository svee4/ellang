using Ellang.Compiler.Lexer;
using Ellang.Compiler.Parser.Nodes;

namespace Ellang.Compiler.Parser.Parselets;

// an assignment is not necessarily to a variable.
// for example: *(ptr) = 5;
public sealed class AssignmentParselet : ParseletBase, IInfixParselet
{
	public Precedence Precedence => Precedence.Assignment;

	public IExpression Parse(Parser parser, IExpression left)
	{
		using var scope = Scope(parser);
		_ = parser.Eat<Equal>();
		return new AssignmentExpression(left, parser.ParseSubExpression(Precedence.Assignment));
	}
}
