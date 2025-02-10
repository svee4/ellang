namespace Ellang.Compiler.Parser.Nodes;

public interface IStatement;

public sealed record StructDeclarationStatement(Identifier Name, NodeList<StructFieldDeclaration> Fields) : IStatement, ITopLevelStatement;
public sealed record StructFieldDeclaration(Identifier Name, TypeRef Type);

public sealed record VariableDeclarationStatement(Identifier Name, TypeRef Type, IExpression Initializer) : IStatement;
public sealed record DiscardStatement(IExpression Expression) : IStatement;

public sealed record EmptyStatement : IStatement;
