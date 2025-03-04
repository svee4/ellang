using Ellang.Compiler.Parser.Nodes;

namespace Ellang.Compiler.AST;

public interface IOperation;

public sealed record VariableDeclarationOperation(VariableSymbol Variable, IOperation? Initializer) : IOperation;
public sealed record InvocationOperation(IFunctionSymbol Target, List<IOperation> Arguments) : IOperation;
public sealed record AssignmentOperation(IOperation Target, IOperation Value) : IOperation;

public abstract record LiteralOperation : IOperation;
public sealed record StringLiteralOperation(string Value) : LiteralOperation;
public sealed record IntegerLiteralOperation(int Value) : LiteralOperation;

public abstract record BinaryOperation(IOperation Left, IOperation Right) : IOperation;

public sealed record AdditionOperation(IOperation Left, IOperation Right) : BinaryOperation(Left, Right);
public sealed record SubtractionOperation(IOperation Left, IOperation Right) : BinaryOperation(Left, Right);
public sealed record MultiplicationOperation(IOperation Left, IOperation Right) : BinaryOperation(Left, Right);
public sealed record DivisionOperation(IOperation Left, IOperation Right) : BinaryOperation(Left, Right);

public sealed record BitwiseAndOperation(IOperation Left, IOperation Right) : BinaryOperation(Left, Right);
public sealed record BitwiseOrOperation(IOperation Left, IOperation Right) : BinaryOperation(Left, Right);
public sealed record BitwiseXorOperation(IOperation Left, IOperation Right) : BinaryOperation(Left, Right);
public sealed record BitwiseLeftShiftOperation(IOperation Left, IOperation Right) : BinaryOperation(Left, Right);
public sealed record BitwiseRightShiftOperation(IOperation Left, IOperation Right) : BinaryOperation(Left, Right);

public sealed record LogicalAndOperation(IOperation Left, IOperation Right) : BinaryOperation(Left, Right);
public sealed record LogicalOrOperation(IOperation Left, IOperation Right) : BinaryOperation(Left, Right);
public sealed record LogicalLessThanOperation(IOperation Left, IOperation Right) : BinaryOperation(Left, Right);
public sealed record LogicalGreaterThanOperation(IOperation Left, IOperation Right) : BinaryOperation(Left, Right);
public sealed record LogicalEqualOperation(IOperation Left, IOperation Right) : BinaryOperation(Left, Right);
public sealed record LogicalNotEqualOperation(IOperation Left, IOperation Right) : BinaryOperation(Left, Right);

public abstract record UnaryOperation(IOperation Source) : IOperation;

public sealed record LogicalNegationOperation(IOperation Source) : UnaryOperation(Source);
public sealed record MathematicalNegationOperation(IOperation Source) : UnaryOperation(Source);
public sealed record BitwiseNotOperation(IOperation Source) : UnaryOperation(Source);
public sealed record DereferenceOperation(IOperation Source) : UnaryOperation(Source);

////////////////////////////

public sealed record LocalVariableReferenceOperation(VariableSymbol Symbol) : IOperation;
public sealed record GlobalVariableReferenceOperation(VariableSymbol Symbol) : IOperation;
public sealed record FunctionReferenceOperation(NamedFunctionSymbol Symbol) : IOperation;
public sealed record StructReferenceOperation(StructSymbol Symbol) : IOperation;
public sealed record TraitReferenceOperation(TraitSymbol Symbol) : IOperation;
