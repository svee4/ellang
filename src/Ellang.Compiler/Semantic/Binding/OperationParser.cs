using Ellang.Compiler.Infra;
using Ellang.Compiler.Parser.Nodes;
using System.ComponentModel.Design;
using System.Diagnostics;

namespace Ellang.Compiler.AST.Binding;

public sealed class OperationParser(Binder analyzer)
{
	private readonly Binder _analyzer = analyzer;

	public IOperation ParseExpression(IExpression expr)
	{
		var visitor = new Visitors.ExpressionVisitor<IOperation>
		{
			FunctionCallExpressionVisitor = (expr, v) =>
			{
				switch (expr.FunctionExpression)
				{
					case IdentifierExpression idexpr:
					{
						var functionIdent = _analyzer.IdentHelper.FromIdentifier(idexpr.Identifier);
						var function = _analyzer.SymbolManager.GlobalFunctionTable.Get(functionIdent);
						var args = expr.Arguments.Select(arg => v.Visit(arg.Value)).ToList();
						return new InvocationOperation(function, args);
					}
					case { } ex: throw new UnreachableException($"Unhandled function call expression type {expr.GetType()}");
					case null: throw new InvalidOperationException("Function call expression argument must not be null");
				};
			},
			AssignmentExpressionVisitor = (expr, v) => new AssignmentOperation(v.Visit(expr.Target), v.Visit(expr.Value)),

			StringLiteralExpressionVisitor = (expr, v) => new StringLiteralOperation(expr.Value),
			IntLiteralExpressionVisitor = (expr, v) => new IntegerLiteralOperation(expr.Value),
			IdentifierExpressionVisitor = (expr, v) =>
			{
				var ident = _analyzer.IdentHelper.FromIdentifier(expr.Identifier);

				if (_analyzer.SymbolManager.LocalVariablesManager.TryGet(expr.Identifier.Value, out var sym))
					return new LocalVariableReferenceOperation(sym);
				else if (_analyzer.SymbolManager.GlobalVariableTable.TryGet(ident, out var sym2))
				{
					return new GlobalVariableReferenceOperation(sym2);
				}
				else if (_analyzer.SymbolManager.GlobalStructTable.TryGet(ident, out var sym3))
				{
					return new StructReferenceOperation(sym3);
				}
				else if (_analyzer.SymbolManager.GlobalTraitTable.TryGet(ident, out var sym4))
				{
					return new TraitReferenceOperation(sym4);
				}
				else if (_analyzer.SymbolManager.GlobalFunctionTable.TryGet(ident, out var sym5))
				{
					return new FunctionReferenceOperation(sym5);
				}
				else
				{
					throw new Binder.CouldNotBindTypeException(expr.Identifier.Value);
				}
			},

			IndexerCallExpressionVisitor = (expr, v) =>
				throw new NotImplementedException("Indexers are not supported (yet)"),

			AdditionExpressionVisitor = (expr, v) => new AdditionOperation(v.Visit(expr.Left), v.Visit(expr.Right)),
			SubtractionExpressionVisitor = (expr, v) => new SubtractionOperation(v.Visit(expr.Left), v.Visit(expr.Right)),
			MultiplicationExpressionVisitor = (expr, v) => v.BinaryVisit(expr, (l, r) => new MultiplicationOperation(l, r)),
			DivisionExpressionVisitor = (expr, v) => v.BinaryVisit(expr, (l, r) => new DivisionOperation(l, r)),

			BitwiseAndExpressionVisitor = (expr, v) => v.BinaryVisit(expr, (l, r) => new BitwiseAndOperation(l, r)),
			BitwiseOrExpressionVisitor = (expr, v) => v.BinaryVisit(expr, (l, r) => new BitwiseOrOperation(l, r)),
			BitwiseXorExpressionVisitor = (expr, v) => v.BinaryVisit(expr, (l, r) => new BitwiseXorOperation(l, r)),
			BitwiseLeftShiftExpressionVisitor = (expr, v) => v.BinaryVisit(expr, (l, r) => new BitwiseLeftShiftOperation(l, r)),
			BitwiseRightShiftExpressionVisitor = (expr, v) => v.BinaryVisit(expr, (l, r) => new BitwiseRightShiftOperation(l, r)),

			LogicalAndExpressionVisitor = (expr, v) => v.BinaryVisit(expr, (l, r) => new LogicalAndOperation(l, r)),
			LogicalOrExpressionVisitor = (expr, v) => v.BinaryVisit(expr, (l, r) => new LogicalOrOperation(l, r)),
			LogicalLessThanExpressionVisitor = (expr, v) => v.BinaryVisit(expr, (l, r) => new LogicalLessThanOperation(l, r)),
			LogicalGreaterThanExpressionVisitor = (expr, v) => v.BinaryVisit(expr, (l, r) => new LogicalGreaterThanOperation(l, r)),
			LogicalEqualExpressionVisitor = (expr, v) => v.BinaryVisit(expr, (l, r) => new LogicalEqualOperation(l, r)),
			LogicalNotEqualExpressionVisitor = (expr, v) => v.BinaryVisit(expr, (l, r) => new LogicalNotEqualOperation(l, r)),

			LogicalNegationExpressionVisitor = (expr, v) => new LogicalNegationOperation(v.Visit(expr)),
			MathematicalNegationExpressionVisitor = (expr, v) => new MathematicalNegationOperation(v.Visit(expr.Source)),
			BitwiseNotExpressionVisitor = (expr, v) => new BitwiseNotOperation(v.Visit(expr.Source)),
			DereferenceExpressionVisitor = (expr, v) => new DereferenceOperation(v.Visit(expr.Source)),
		};

		return visitor.Visit(expr);
	}
}
