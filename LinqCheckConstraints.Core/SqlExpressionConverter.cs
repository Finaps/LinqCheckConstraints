using System.Linq.Expressions;
using System.Text;

namespace Finaps.LinqCheckConstraints.Core;

public abstract class SqlExpressionConverter : ExpressionVisitor
{
  private readonly StringBuilder _builder = new();

  public string Convert(Expression expression)
  {
    _builder.Clear();
    Visit(expression);
    return _builder.ToString();
  }

  protected virtual string ConvertUnary(UnaryExpression u) =>
    u.NodeType switch
    {
      ExpressionType.Not => "NOT",
      ExpressionType.Convert => "",
      _ => throw new NotSupportedException($"The unary operator '{u.NodeType}' is not supported")
    };

  protected virtual string ConvertBinary(BinaryExpression b) =>
    b.NodeType switch
    {
      ExpressionType.And or ExpressionType.AndAlso => "AND",
      ExpressionType.Or or ExpressionType.OrElse => "OR",
      ExpressionType.Equal => IsNullConstant(b.Right) ? "IS" : "=",
      ExpressionType.NotEqual => IsNullConstant(b.Right) ? "IS NOT" : "<>",
      ExpressionType.LessThan => "<",
      ExpressionType.LessThanOrEqual => "<=",
      ExpressionType.GreaterThan => ">",
      ExpressionType.GreaterThanOrEqual => ">=",
      _ => throw new NotSupportedException($"The binary operator '{b.NodeType}' is not supported")
    };

  protected virtual object ConvertConstant(ConstantExpression c) =>
    c.Value == null
      ? "NULL"
      : Type.GetTypeCode(c.Value.GetType()) switch
      {
        TypeCode.Empty => "NULL",
        TypeCode.Boolean => (bool) c.Value ? 1 : 0,
        TypeCode.String or TypeCode.DateTime => $"'{c.Value}'",
        TypeCode.SByte => (sbyte) c.Value,
        TypeCode.Int16 => (short) c.Value,
        TypeCode.Int32 => (int) c.Value,
        TypeCode.Int64 => (long) c.Value,
        TypeCode.Byte => (byte) c.Value,
        TypeCode.UInt16 => (ushort) c.Value,
        TypeCode.UInt32 => (uint) c.Value,
        TypeCode.UInt64 => (ulong) c.Value,
        TypeCode.Object => throw new NotSupportedException($"The constant for '{c.Value}' is not supported"),
        _ => c.Value
      };

  protected override Expression VisitUnary(UnaryExpression u)
  {
    _builder.Append($" {ConvertUnary(u)} ");
    Visit(u.Operand);
    return u;
  }

  protected override Expression VisitBinary(BinaryExpression b)
  {
    _builder.Append('(');

    Visit(b.Left);

    _builder.Append($" {ConvertBinary(b)} ");

    Visit(b.Right);
    _builder.Append(')');
    return b;
  }

  protected override Expression VisitConstant(ConstantExpression c)
  {
    _builder.Append(ConvertConstant(c));
    return c;
  }

  protected override Expression VisitMember(MemberExpression m)
  {
    if (m.Expression is not { NodeType: ExpressionType.Parameter })
      throw new NotSupportedException($"The member '{m.Member.Name}' is not supported");

    _builder.Append($"\"{m.Member.Name}\"");
    return m;
  }

  private static bool IsNullConstant(Expression exp)
  {
    return exp.NodeType == ExpressionType.Constant && ((ConstantExpression)exp).Value == null;
  }
}