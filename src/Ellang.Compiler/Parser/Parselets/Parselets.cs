using Ellang.Compiler.Infra;
using Ellang.Compiler.Parser.Nodes;

namespace Ellang.Compiler.Parser.Parselets;

public interface IPrefixParselet
{
	IExpression Parse(Parser parser);
}

public interface IInfixParselet
{
	IExpression Parse(Parser parser, IExpression left);
	Precedence Precedence { get; }
}

public enum Precedence
{
	None = 0,
	Assignment = 1,
	AdditionOrSubtraction,
	MultiplicationOrDivision,
	Prefix,
	Postfix,
	FunctionCall
}

public abstract class ParseletBase
{
	protected IDisposable? Scope(Parser parser) => parser.Scope(Helpers.GetPrettyTypeName(GetType()));
}

