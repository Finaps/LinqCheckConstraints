using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace Finaps.LinqCheckConstraints.Core;

public abstract class SqlExpressionConverter : ExpressionVisitor
{
  private readonly StringBuilder _builder = new();
  private Type? _declaringType;

  public string Convert<TEntity>(Expression<Func<TEntity, bool>> expression)
  {
    _builder.Clear();
    _declaringType = typeof(TEntity);

    Visit(expression);
    
    return _builder.ToString();
  }

  protected virtual string ConvertUnary(UnaryExpression u) =>
    u.NodeType switch
    {
      ExpressionType.Not => "NOT",
      _ => throw new NotSupportedException($"The unary operator '{u.NodeType}' is not supported")
    };

  protected virtual string ConvertBinary(BinaryExpression b) =>
    b.NodeType switch
    {
      ExpressionType.And => "&",
      ExpressionType.AndAlso => "AND",
      ExpressionType.Or => "|",
      ExpressionType.OrElse => "OR",
      
      ExpressionType.Equal => IsNullConstant(b.Right) ? "IS" : "=",
      ExpressionType.NotEqual => IsNullConstant(b.Right) ? "IS NOT" : "<>",
      
      ExpressionType.LessThan => "<",
      ExpressionType.LessThanOrEqual => "<=",
      ExpressionType.GreaterThan => ">",
      ExpressionType.GreaterThanOrEqual => ">=",
      
      _ => throw new NotSupportedException($"The binary operator '{b.NodeType}' is not supported")
    };

  protected virtual object ConvertObject(object? obj) =>
    obj switch
    {
      null => "NULL",
      bool x => x,
      sbyte x => x,
      short x => x,
      int x => x,
      long x => x,
      byte x => x,
      ushort x => x,
      uint x => x,
      ulong x => x,
      decimal x => x,
      string x => Quote(x),
      Guid x => Quote(x),
      DateTime x => Quote(x),
      DateTimeOffset x => Quote(x),
      _ => throw new NotSupportedException($"The constant for '{obj}' is not supported")
    };

  protected override Expression VisitUnary(UnaryExpression u)
  {
    if (u.NodeType != ExpressionType.Convert)
      _builder.Append($" {ConvertUnary(u)} ");
    
    Visit(u.Operand);
    return u;
  }

  protected override Expression VisitBinary(BinaryExpression b)
  {
    var doesNotNeedBrackets =
      b.Left.NodeType is ExpressionType.Constant or ExpressionType.MemberAccess or ExpressionType.Convert &&
      b.Right.NodeType is ExpressionType.Constant or ExpressionType.MemberAccess or ExpressionType.Convert;

    if (!doesNotNeedBrackets) _builder.Append('(');

    Visit(b.Left);

    _builder.Append($" {ConvertBinary(b)} ");

    Visit(b.Right);
    
    if (!doesNotNeedBrackets) _builder.Append(')');
    
    return b;
  }

  protected override Expression VisitConstant(ConstantExpression c)
  {
    _builder.Append(ConvertObject(c.Value));
    return c;
  }

  protected override Expression VisitMember(MemberExpression m)
  {
    _builder.Append(IsNavigationExpression(m) ? $"\"{m.Member.Name}\"" : ConvertObject(GetValue(m)));

    return m;
  }

  protected override Expression VisitMethodCall(MethodCallExpression node)
  {
    var obj = node.Object == null ? null : GetValue(node.Object);

    switch (obj)
    {
      case Regex regex when node.Method.Name is nameof(Regex.IsMatch):
        if (node.Arguments.SingleOrDefault() is MemberExpression expression && IsNavigationExpression(expression))
          _builder.Append($"\"{expression.Member.Name}\" ~* '{regex.ToString()}'");
        else
          throw new NotSupportedException($"First argument to {nameof(Regex)}.{nameof(Regex.IsMatch)} must be a navigation expression");
        break;
      default:
        throw new NotSupportedException($"Method {obj}.{node.Method.Name} is not supported");
    }

    return node;
  }

  private static object GetValue(Expression member) =>
    Expression.Lambda<Func<object>>(Expression.Convert(member, typeof(object))).Compile()();

  private bool IsNavigationExpression(MemberExpression expression) =>
    expression.Expression?.NodeType == ExpressionType.Parameter && expression.Member.DeclaringType == _declaringType;

  private static bool IsNullConstant(Expression exp)
  {
    return exp.NodeType == ExpressionType.Constant && ((ConstantExpression)exp).Value == null;
  }

  private static string Quote(object x) => $"'{x}'";
}