
using Ellang.Compiler.Lexer;

namespace Ellang.Compiler.Parser;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "")]
public sealed class ParserException : Exception
{
	public LexerToken? Token { get; }

	public ParserException(string message) : base(message) { }

	public ParserException(LexerToken token, string message) : base(message) =>
		Token = token;
}
