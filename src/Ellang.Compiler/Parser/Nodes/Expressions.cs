namespace Ellang.Compiler.Parser.Nodes;

public interface IExpression;

public sealed record StringLiteralExpression(string Value) : IExpression;
public sealed record IntLiteralExpression(int Value) : IExpression;

public sealed record IdentifierExpression(Identifier Identifier) : IExpression;
public sealed record IndexerCallExpression(IExpression Source, IExpression Indexer) : IExpression;
public sealed record MemberAccessExpression(IExpression Source, IdentifierExpression Member) : IExpression;

public abstract record BinaryExpression(IExpression Left, IExpression Right) : IExpression;

public sealed record AdditionExpression(IExpression Left, IExpression Right) : BinaryExpression(Left, Right);
public sealed record SubtractionExpression(IExpression Left, IExpression Right) : BinaryExpression(Left, Right);
public sealed record MultiplicationExpression(IExpression Left, IExpression Right) : BinaryExpression(Left, Right);
public sealed record DivisionExpression(IExpression Left, IExpression Right) : BinaryExpression(Left, Right);

public sealed record BitwiseAndExpression(IExpression Left, IExpression Right) : BinaryExpression(Left, Right);
public sealed record BitwiseOrExpression(IExpression Left, IExpression Right) : BinaryExpression(Left, Right);
public sealed record BitwiseXorExpression(IExpression Left, IExpression Right) : BinaryExpression(Left, Right);
public sealed record BitwiseLeftShiftExpression(IExpression Left, IExpression Right) : BinaryExpression(Left, Right);
public sealed record BitwiseRightShiftExpression(IExpression Left, IExpression Right) : BinaryExpression(Left, Right);
public sealed record LogicalAndExpression(IExpression Left, IExpression Right) : BinaryExpression(Left, Right);
public sealed record LogicalOrExpression(IExpression Left, IExpression Right) : BinaryExpression(Left, Right);
public sealed record LogicalLessThanExpression(IExpression Left, IExpression Right) : BinaryExpression(Left, Right);
public sealed record LogicalGreaterThanExpression(IExpression Left, IExpression Right) : BinaryExpression(Left, Right);
public sealed record LogicalEqualExpression(IExpression Left, IExpression Right) : BinaryExpression(Left, Right);
public sealed record LogicalNotEqualExpression(IExpression Left, IExpression Right) : BinaryExpression(Left, Right);

public abstract record UnaryExpression : IExpression;
public abstract record PrefixUnaryExpression : UnaryExpression;

public sealed record LogicalNegationExpression(IExpression Source) : PrefixUnaryExpression;
public sealed record MathematicalNegationExpression(IExpression Source) : PrefixUnaryExpression;
public sealed record BitwiseNotExpression(IExpression Source) : PrefixUnaryExpression;
public sealed record DereferenceExpression(IExpression Source) : PrefixUnaryExpression;
