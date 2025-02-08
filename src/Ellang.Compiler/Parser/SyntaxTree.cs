using Ellang.Compiler.Parser.Nodes;
using System.Globalization;
using System.Text;

namespace Ellang.Compiler.Parser;

public sealed class SyntaxTree
{
	public List<ITopLevelStatement> TopLevelStatements { get; } = [];

	// everything below this line is TEMPORARY and subject to great refactoring

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0058:Expression value is never used", Justification = "Noise")]
	public string Reconstruct()
	{
		var w = new IndentedTextWriter();
		foreach (var statement in TopLevelStatements)
		{
			w.AppendLine();
			w.AppendLine();

			switch (statement)
			{
				case FunctionExpressionStatement func:
				{
					w.Append("func ");
					w.Append(func.Name.Value);

					w.Append('(');
					foreach (var p in func.Parameters.Nodes)
					{
						w.Append(p.Name.Value);
						w.Append(": ");
						w.Append(TypeRefToString(p.Type));
						w.Append(", ");
					}
					w.StringBuilder.Remove(w.StringBuilder.Length - 2, 2);

					w.Append(')');
					w.Append(": ");
					w.Append(TypeRefToString(func.ReturnType));
					w.Append(" {");
					w.AppendLine();
					w.AddIndent();

					foreach (var st in func.Statements.Nodes)
					{
						w.AppendIndentation();
						switch (st)
						{
							case VariableDeclarationStatement varDecl:
							{
								w.Append("var ");
								w.Append(varDecl.Name.Value);
								w.Append(": ");
								w.Append(TypeRefToString(varDecl.Type));
								w.Append(" = ");
								w.Append(ExpressionToString(varDecl.Initializer));
								w.Append(';');
								w.AppendLine();
								break;
							}
							default: throw new NotSupportedException();
						}
					}

					w.RemoveIndent();
					w.AppendLine();
					w.Append('}');

					break;
				}
				case StructDeclarationStatement s:
				{
					w.Append("struct ");
					w.Append(s.Name.Value);
					w.Append('{');
					w.AppendLine();
					w.AddIndent();

					foreach (var field in s.Fields.Nodes)
					{
						w.AppendIndentation();
						w.Append(field.Name.Value);
						w.Append(": ");
						w.Append(TypeRefToString(field.Type));
						w.Append(';');
						w.AppendLine();
					}

					w.RemoveIndent();
					w.Append('}');
					break;
				}
				default: throw new NotSupportedException();
			}
		}

		return w.StringBuilder.ToString();

		static string TypeRefToString(TypeRef typeRef) => typeRef switch
		{
			PlainTypeRef p => p.Identifier.Value,
			RefTypeRef r => new string('&', r.ReferenceCount) + r.Identifier.Value,
			_ => throw new NotSupportedException()
		};

		static string ExpressionToString(IExpression expr) => expr switch
		{
			StringLiteralExpression s => $"\"{s.Value}\"",
			IntLiteralExpression i => i.Value.ToString(CultureInfo.InvariantCulture),
			IdentifierExpression ident => ident.Identifier.Value,
			AssignmentExpression ass => $"{ExpressionToString(ass.Target)} = {ExpressionToString(ass.Value)}",

			BinaryExpression => expr switch
			{
				AdditionExpression op => $"{ExpressionToString(op.Left)} + {ExpressionToString(op.Right)}",
				SubtractionExpression op => $"{ExpressionToString(op.Left)} - {ExpressionToString(op.Right)}",
				MultiplicationExpression op => $"{ExpressionToString(op.Left)} * {ExpressionToString(op.Right)}",
				DivisionExpression op => $"{ExpressionToString(op.Left)} / {ExpressionToString(op.Right)}",
				BitwiseAndExpression op => $"{ExpressionToString(op.Left)} & {ExpressionToString(op.Right)}",
				BitwiseOrExpression op => $"{ExpressionToString(op.Left)} | {ExpressionToString(op.Right)}",
				BitwiseXorExpression op => $"{ExpressionToString(op.Left)} ^ {ExpressionToString(op.Right)}",
				BitwiseLeftShiftExpression op => $"{ExpressionToString(op.Left)} << {ExpressionToString(op.Right)}",
				BitwiseRightShiftExpression op => $"{ExpressionToString(op.Left)} >> {ExpressionToString(op.Right)}",
				_ => throw new NotSupportedException($"Unsupported binary expression {expr}")
			},

			PrefixUnaryExpression => expr switch
			{
				LogicalNegationExpression op => $"!{ExpressionToString(op.Source)}",
				MathematicalNegationExpression op => $"-{ExpressionToString(op.Source)}",
				BitwiseNotExpression op => $"~{ExpressionToString(op.Source)}",
				DereferenceExpression op => $"*{ExpressionToString(op.Source)}",
				_ => throw new NotSupportedException($"Unsupported prefix unary expression {expr}")
			},

			_ => throw new NotSupportedException($"Unsupported expression {expr}")
		};
	}

	private sealed class IndentedTextWriter
	{
		public StringBuilder StringBuilder { get; } = new();

		public int Indent { get; private set; }

		public void AppendIndentation() => StringBuilder.Append(new string('\t', Indent));

		public IndentedTextWriter Append(char value)
		{
			StringBuilder.Append(value);
			return this;
		}

		public IndentedTextWriter Append(string value)
		{
			StringBuilder.Append(value);
			return this;
		}

		public IndentedTextWriter AppendLine()
		{
			StringBuilder.AppendLine();
			return this;
		}

		public IndentedTextWriter AppendLine(string value)
		{
			StringBuilder.AppendLine(value);
			return this;
		}

		public IndentedTextWriter AddIndent()
		{
			Indent++;
			return this;
		}

		public IndentedTextWriter RemoveIndent()
		{
			Indent--;
			return this;
		}

		public IDisposable WithIndent()
		{
			AddIndent();
			return new Unindenter(this);
		}

		public readonly struct Unindenter(IndentedTextWriter writer) : IDisposable
		{
			public void Dispose() => writer.RemoveIndent();
		}
	}
}
