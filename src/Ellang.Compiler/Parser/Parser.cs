using Ellang.Compiler.Infra;
using Ellang.Compiler.Lexer;
using Ellang.Compiler.Parser.Nodes;
using Ellang.Compiler.Parser.Parselets;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Ellang.Compiler.Parser;

/*
 * Primary parser is a recursive descent parser.
 * Expression are parsed using operator precedence parsing based on this article
 * https://journal.stuffwithstuff.com/2011/03/19/pratt-parsers-expression-parsing-made-easy/
 * and this c# implementation
 * https://github.com/jfcardinal/BantamCs
 * which is encapsulated in ExpressionParser.
 * 
 * https://en.wikipedia.org/wiki/Recursive_descent_parser
 * https://en.wikipedia.org/wiki/Operator-precedence_parser
*/

public sealed class Parser
{
	private List<LexerToken> _tokens = null!;
	private int _position;

	internal ExpressionParser ExpressionParser { get; }
	internal ILogger<Parser> Logger { get; }

	public Parser(ILogger<Parser> logger)
	{
		Logger = logger;
		ExpressionParser = new ExpressionParser(this);
	}

	public SyntaxTree Parse(List<LexerToken> tokens)
	{
		_tokens = tokens;

		var tree = new SyntaxTree();
		while (Peek() is not EndOfFile)
		{
			tree.TopLevelStatements.Add(ParseTopLevelStatement());
		}

		return tree;
	}

	public LexerToken Peek() => _tokens[_position];
	public LexerToken Peek(int skip) => _tokens[_position + skip];
	public LexerToken Eat() => _tokens[_position++];

	public T Peek<T>() where T : LexerToken =>
		Peek() is T v ? v : ThrowAt<T>(Peek(), "Expected {Expected}, got {Actual}", typeof(T), Peek().GetType());

	public T Eat<T>() where T : LexerToken =>
		Peek() is T ? (T)Eat() : ThrowAt<T>(Peek(), "Expected {Expected}, got {Actual}", typeof(T), Eat().GetType());

	public T? EatIf<T>() where T : LexerToken =>
		Peek() is T ? Eat<T>() : null;

	public IDisposable? Scope([CallerMemberName] string scope = "(unknown caller)") =>
		Logger.BeginScope(scope);

	public ITopLevelStatement ParseTopLevelStatement()
	{
		using var scope = Scope();
		return Peek() switch
		{
			FuncKeyword => ParseFunction(),
			StructKeyword => ParseStruct(),
			var token => ThrowAt<ITopLevelStatement>(token, "Expected func or struct, got {Actual}", token.GetType())
		};
	}

	public FunctionExpressionStatement ParseFunction()
	{
		using var scope = Scope();

		_ = Eat<FuncKeyword>();
		var name = Eat<IdentifierLiteral>();

		List<FunctionParameter> parameters = [];
		using (Scope("[Parameters]"))
		{
			_ = Eat<OpenParen>();

			while (Peek() is not CloseParen)
			{
				parameters.Add(ParseFunctionParameter());
				_ = EatIf<Comma>();
			}

			_ = Eat<CloseParen>();
		}

		_ = Eat<Colon>();
		var returnType = ParseTypeRef();

		List<IStatement> statements = [];
		using (Scope("[Statements]"))
		{
			_ = Eat<OpenBrace>();

			while (Peek() is not CloseBrace)
			{
				statements.Add(ParseStatement());
			}

			_ = Eat<CloseBrace>();
		}

		return new FunctionExpressionStatement(
			returnType,
			new Identifier(name.Value),
			NodeList.From(parameters),
			NodeList.From(statements));

		FunctionParameter ParseFunctionParameter()
		{
			using var scope = Scope(nameof(ParseFunctionParameter));

			var identifier = Eat<IdentifierLiteral>().Value;
			_ = Eat<Colon>();
			var type = ParseTypeRef();

			return new FunctionParameter(new Identifier(identifier), type);
		}
	}

	public IStatement ParseStatement()
	{
		using var scope = Scope();

		switch (Peek())
		{
			case SemiColon:
			{
				_ = Eat<SemiColon>();
				return new EmptyStatement();
			}
			case FuncKeyword:
			{
				return ParseFunction();
			}
			case VarKeyword:
			{
				var expr = ParseVariableDeclaration();
				_ = Eat<SemiColon>();
				return expr;
			}
			case UnderscoreKeyword:
			{
				_ = Eat<UnderscoreKeyword>();
				_ = Eat<Equal>();
				var expr = ParseExpression();
				_ = Eat<SemiColon>();
				return new DiscardStatement(expr);
			}
			default:
			{
				ThrowAt(Peek(), "Expected statement");
				throw new UnreachableException();
			}
		}
	}

	public IExpression ParseExpression()
	{
		using var scope = Scope();
		return ExpressionParser.ParseExpression(Precedence.None);
	}

	// convenience shortcut
	public IExpression ParseSubExpression(Precedence precedence) =>
		ExpressionParser.ParseExpression(precedence);

	public StructDeclarationStatement ParseStruct()
	{
		using var scope = Scope();

		_ = Eat<StructKeyword>();
		var structName = Eat<IdentifierLiteral>().Value;
		_ = Eat<OpenBrace>();

		List<StructFieldDeclaration> fields = [];
		while (Peek() is not CloseBrace)
		{
			var fieldName = Eat<IdentifierLiteral>().Value;
			_ = Eat<Colon>();

			var type = ParseTypeRef();
			_ = Eat<SemiColon>();

			fields.Add(new StructFieldDeclaration(new Identifier(fieldName), type));
		}

		_ = Eat<CloseBrace>();

		return new StructDeclarationStatement(new Identifier(structName), NodeList.From(fields));
	}

	public VariableDeclarationStatement ParseVariableDeclaration()
	{
		using var scope = Scope();

		_ = Eat<VarKeyword>();
		var name = Eat<IdentifierLiteral>().Value;

		_ = Eat<Colon>();
		var type = ParseTypeRef();

		_ = Eat<Equal>();
		var expr = ParseExpression();

		return new VariableDeclarationStatement(new Identifier(name), type, expr);
	}

	public TypeRef ParseTypeRef()
	{
		using var scope = Scope();

		var refCount = 0;
		while (EatIf<Ampersand>() is not null)
		{
			refCount++;
		}

		var identifier = Eat<IdentifierLiteral>().Value;

		List<TypeRef> generics = [];
		if (EatIf<OpenAngleBracket>() is not null)
		{
			generics.Add(ParseTypeRef());
			_ = Eat<CloseAngleBracket>();
		}

		return new TypeRef(new Identifier(identifier), refCount, generics);
	}

	[SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "Close enough")]
	[DoesNotReturn]
	internal T Throw<T>(int line, int column, string format, params object?[] args)
	{
		format = $"(at {{Line}}:{{Column}}): {format}";
		args = [line, column, .. args];

		Logger.LogError(format, args);
		throw new ParserException(new LogValuesFormatter(format).Format(args));
	}

	internal void Throw(int line, int column, string format, params object?[] args) => Throw<object>(line, column, format, args);

	internal T ThrowAt<T>(LexerToken token, string format, params object?[] args) =>
		Throw<T>(token.Line, token.Column, format, args);

	internal void ThrowAt(LexerToken token, string format, params object?[] args) => 
		Throw<object>(token.Line, token.Column, format, args);

}
