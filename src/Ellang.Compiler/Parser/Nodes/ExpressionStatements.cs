namespace Ellang.Compiler.Parser.Nodes;

public interface IExpressionStatement : IExpression, IStatement;

public sealed record FunctionExpressionStatement(TypeRef ReturnType, Identifier Name, NodeList<FunctionParameter> Parameters, NodeList<IStatement> Statements) : IExpressionStatement, ITopLevelStatement;
public sealed record FunctionParameter(Identifier Name, TypeRef Type);

public sealed record FunctionCallExpression(IExpression FunctionExpression, List<FunctionArgument> Arguments) : IExpressionStatement;
public sealed record FunctionArgument(IExpression Value);

public sealed record AssignmentExpression(IExpression Target, IExpression Value) : IExpressionStatement;
