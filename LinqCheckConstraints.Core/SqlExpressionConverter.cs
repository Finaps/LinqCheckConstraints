using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Finaps.LinqCheckConstraints.Core;

public abstract class SqlExpressionConverter : ExpressionVisitor
{
  private readonly List<string> _tokens = new();
  private Type? _declaringType;

  public string Convert<TEntity>(Expression<Func<TEntity, bool>> expression)
  {
    _tokens.Clear();
    _declaringType = typeof(TEntity);

    Visit(expression);

    return string.Join(" ", _tokens).Replace("( ", "(").Replace(" )", ")");
  }

  protected virtual string ConvertUnary(UnaryExpression node) =>
    node.NodeType switch
    {
      ExpressionType.Not => "NOT",
      ExpressionType.Negate => "-",
      _ => throw new NotSupportedException($"The unary operator '{node.NodeType}' is not supported")
    };

  protected virtual string ConvertBinary(BinaryExpression node) =>
    node.NodeType switch
    {
      ExpressionType.And => "&",
      ExpressionType.AndAlso => "AND",
      ExpressionType.Or => "|",
      ExpressionType.OrElse => "OR",
      
      ExpressionType.Equal => IsNullConstant(node.Right) ? "IS" : "=",
      ExpressionType.NotEqual => IsNullConstant(node.Right) ? "IS NOT" : "<>",
      
      ExpressionType.LessThan => "<",
      ExpressionType.LessThanOrEqual => "<=",
      ExpressionType.GreaterThan => ">",
      ExpressionType.GreaterThanOrEqual => ">=",
      
      ExpressionType.Add => "+",
      ExpressionType.Subtract => "-",
      ExpressionType.Multiply => "*",
      ExpressionType.Divide => "/",
      ExpressionType.Modulo => "%",
      ExpressionType.Power => "^",
      
      _ => throw new NotSupportedException($"The binary operator '{node.NodeType}' is not supported")
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
      string x => SingleQuote(x),
      Guid x => SingleQuote(x),
      DateTime x => SingleQuote(x),
      DateTimeOffset x => SingleQuote(x),
      _ => throw new NotSupportedException($"The constant for '{obj}' is not supported")
    };

  // https://www.postgresql.org/docs/current/sql-syntax-lexical.html#SQL-PRECEDENCE-TABLE
  protected virtual byte Precedence(ExpressionType type) =>
    type switch
    {
      ExpressionType.OrElse => 0,
      ExpressionType.AndAlso => 1,
      ExpressionType.Not => 2,
      ExpressionType.LessThan or ExpressionType.LessThanOrEqual => 3,
      ExpressionType.Equal or ExpressionType.NotEqual => 3,
      ExpressionType.GreaterThan or ExpressionType.GreaterThanOrEqual => 3,
      // (any other operator) => 4
      ExpressionType.Add or ExpressionType.Subtract => 5,
      ExpressionType.Multiply or ExpressionType.Divide or ExpressionType.Modulo => 6,
      ExpressionType.Power => 7,
      ExpressionType.UnaryPlus or ExpressionType.Negate => 8,
      ExpressionType.Index => 9,
      ExpressionType.Convert => 10,
      ExpressionType.MemberAccess or ExpressionType.Constant => 11,
      _ => 4
    };
  
  public void Visit(Expression? node, bool brackets)
  {
    if (brackets) _tokens.Add("(");
    base.Visit(node);
    if (brackets) _tokens.Add(")");
  }

  protected override Expression VisitUnary(UnaryExpression node)
  {
    if (node.NodeType != ExpressionType.Convert)
      _tokens.Add(ConvertUnary(node));

    Visit(node.Operand);
    return node;
  }

  protected override Expression VisitBinary(BinaryExpression node)
  {
    Visit(node.Left, Precedence(node.NodeType) > Precedence(node.Left.NodeType));

    _tokens.Add(ConvertBinary(node));

    Visit(node.Right, Precedence(node.NodeType) >= Precedence(node.Right.NodeType));

    return node;
  }

  protected override Expression VisitConditional(ConditionalExpression node)
  {
    _tokens.Add("CASE WHEN");

    Visit(node.Test);
    
    _tokens.Add("THEN");

    Visit(node.IfTrue);
    
    _tokens.Add("ELSE");

    Visit(node.IfFalse);
    
    _tokens.Add("END");
    
    return node;
  }

  protected override Expression VisitSwitch(SwitchExpression node)
  {
    throw new NotSupportedException();
  }

  protected override Expression VisitConstant(ConstantExpression node)
  {
    _tokens.Add(ConvertObject(node.Value).ToString()!);
    return node;
  }

  protected override Expression VisitMember(MemberExpression node)
  {
    _tokens.Add(IsNavigationExpression(node) 
      ? DoubleQuote(node.Member.Name)
      : ConvertObject(GetValue(node)).ToString()!);

    return node;
  }

  protected override Expression VisitMethodCall(MethodCallExpression node)
  {
    var obj = node.Object == null ? null : GetValue(node.Object);

    switch (obj)
    {
      case Regex regex when node.Method.Name is nameof(Regex.IsMatch):
        if (node.Arguments.SingleOrDefault() is MemberExpression expression && IsNavigationExpression(expression))
          _tokens.AddRange(new []{ DoubleQuote(expression.Member.Name), "~*", SingleQuote(regex.ToString()) });
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

  protected static string SingleQuote(object x) => $"'{x}'";
  protected static string DoubleQuote(object x) => $"\"{x}\"";
}