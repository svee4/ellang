namespace Ellang.Compiler.Parser.Nodes;

public interface IStatement;

public sealed record FunctionDeclarationStatement(TypeRef ReturnType, Identifier Name, NodeList<string> TypeParameters,
	NodeList<FunctionParameter> Parameters, NodeList<IStatement> Statements) : IStatement, ITopLevelStatement;

public sealed record FunctionParameter(Identifier Name, TypeRef Type);

public sealed record StructDeclarationStatement(Identifier Name, NodeList<string> TypeParameters, 
	NodeList<StructFieldDeclaration> Fields) : IStatement, ITopLevelStatement;

public sealed record StructFieldDeclaration(Identifier Name, TypeRef Type);

public sealed record ImplBlockStatement(StructMethodDeclarationStatement Methods);

public sealed record StructMethodDeclarationStatement(TypeRef ReturnType, Identifier Name, NodeList<string> TypeParameters,
	NodeList<FunctionParameter> Parameters, NodeList<IStatement> Statemets) : IStatement;

public sealed record VariableDeclarationStatement(Identifier Name, TypeRef Type, IExpression Initializer) : IStatement;
public sealed record DiscardStatement(IExpression Expression) : IStatement;

public sealed record TraitDeclarationStatement(Identifier Name) : ITopLevelStatement;

public sealed record EmptyStatement : IStatement;
