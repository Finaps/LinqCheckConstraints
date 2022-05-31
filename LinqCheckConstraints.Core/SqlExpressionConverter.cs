using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Finaps.LinqCheckConstraints.Core;

public class SqlExpressionConverter : ExpressionVisitor
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
      
      ExpressionType.Add => node.Type == typeof(string) ? "||" : "+",
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
  protected virtual int Precedence(Expression expression) =>
    expression.NodeType switch
    {
      ExpressionType.OrElse => -4,
      ExpressionType.AndAlso => -3,
      ExpressionType.Not => -2,
      ExpressionType.LessThan or ExpressionType.LessThanOrEqual => -1,
      ExpressionType.Equal or ExpressionType.NotEqual => -1,
      ExpressionType.GreaterThan or ExpressionType.GreaterThanOrEqual => -1,
      // (any other operator) => 0
      ExpressionType.Add or ExpressionType.Subtract => 1,
      ExpressionType.Multiply or ExpressionType.Divide or ExpressionType.Modulo => 2,
      ExpressionType.Power => 3,
      ExpressionType.UnaryPlus or ExpressionType.Negate => 4,
      ExpressionType.Index => 5,
      ExpressionType.Convert => 6,
      ExpressionType.MemberAccess or ExpressionType.Constant => 7,
      _ => 0
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

    Visit(node.Operand, Precedence(node) > Precedence(node.Operand));
    return node;
  }

  protected override Expression VisitBinary(BinaryExpression node)
  {
    Visit(node.Left, Precedence(node) > Precedence(node.Left));

    _tokens.Add(ConvertBinary(node));

    Visit(node.Right, Precedence(node) >= Precedence(node.Right));

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
    switch (IsParameterAccess(node))
    {
      // When current node accesses a parameter member (e.g. x => x.A) -> resolve as "A"
      case true when node.Expression is { NodeType: ExpressionType.Parameter }:
        _tokens.Add(DoubleQuote(node.Member.Name));
        break;
      
      // When current node accesses the str.Length member -> resolve as char_length(str)
      case true or false when node.Member.DeclaringType == typeof(string) && node.Member.Name == nameof(string.Length):
        _tokens.Add("char_length(");
        Visit(node.Expression);
        _tokens.Add(")");
        break;
      
      // When current node accesses non parameter member -> resolve object
      case false:
        _tokens.Add(ConvertObject(GetValue(node)).ToString()!);
        break;
      
      // Otherwise, member access is not supported
      default:
        throw new NotSupportedException($"Member {node.Member.DeclaringType}.{node.Member.Name} is not supported");
    }

    return node;
  }

  protected override Expression VisitMethodCall(MethodCallExpression node)
  {
    switch (node.Method.Name)
    {
      case nameof(Regex.IsMatch) when node.Object != null && GetValue(node.Object) is Regex regex:
        VisitRegex(node.Arguments.Single(), regex);
        break;
      case nameof(Regex.IsMatch) when node.Method.DeclaringType == typeof(Regex):
        VisitRegex(node.Arguments.First(), node.Arguments.Skip(1).Single());
        break;
      case nameof(string.ToLower):
        _tokens.Add("lower(");
        Visit(node.Object);
        _tokens.Add(")");
        break;
      case nameof(string.ToUpper):
        _tokens.Add("upper(");
        Visit(node.Object);
        _tokens.Add(")");
        break;
      default:
        throw new NotSupportedException($"Method {node.Method.DeclaringType}.{node.Method.Name} is not supported");
    }

    return node;
  }

  protected virtual void VisitRegex(Expression argument, Regex regex)
  {
    Visit(argument, 0 > Precedence(argument));
    _tokens.AddRange(new[] { "~", SingleQuote(regex) });
  }

  protected virtual void VisitRegex(Expression argument, Expression regex)
  {
    Visit(argument, 0 > Precedence(argument));
    _tokens.Add("~");
    Visit(regex, 0 > Precedence(regex));
  }

  private static object GetValue(Expression member) =>
    Expression.Lambda<Func<object>>(Expression.Convert(member, typeof(object))).Compile()();

  private static bool IsParameterAccess(MemberExpression? e) =>
    e != null && 
    (e.Expression is { NodeType: ExpressionType.Parameter } || IsParameterAccess(e.Expression as MemberExpression));

  private static bool IsNullConstant(Expression exp)
  {
    return exp.NodeType == ExpressionType.Constant && ((ConstantExpression)exp).Value == null;
  }

  protected static string SingleQuote(object x) => $"'{x}'";
  protected static string DoubleQuote(object x) => $"\"{x}\"";
}