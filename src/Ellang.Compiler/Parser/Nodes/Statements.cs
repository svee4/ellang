using Ellang.Compiler.Infra;

namespace Ellang.Compiler.Parser.Nodes;

public interface IStatement;

public sealed record FunctionDeclarationStatement(TypeRef ReturnType, Identifier Name, EquatableList<string> TypeParameters,
	EquatableList<FunctionParameter> Parameters, EquatableList<IExpressionStatement> Statements) : IStatement, ITopLevelStatement;

public sealed record FunctionParameter(Identifier Name, TypeRef Type);

public sealed record StructDeclarationStatement(Identifier Name, EquatableList<string> TypeParameters,
	EquatableList<StructFieldDeclaration> Fields) : IStatement, ITopLevelStatement;

public sealed record StructFieldDeclaration(Identifier Name, TypeRef Type);

public sealed record ImplBlockStatement(StructMethodDeclarationStatement Methods);

public sealed record StructMethodDeclarationStatement(TypeRef ReturnType, Identifier Name, EquatableList<string> TypeParameters,
	EquatableList<FunctionParameter> Parameters, EquatableList<IStatement> Statemets) : IStatement;

public sealed record VariableDeclarationStatement(Identifier Name, TypeRef Type, IExpression Initializer) 
	: IExpressionStatement;

public sealed record TraitDeclarationStatement(Identifier Name) : ITopLevelStatement;
