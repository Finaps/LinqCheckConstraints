using System.Linq.Expressions;

namespace Finaps.LinqCheckConstraints.Core;

public class ExpressionPropertiesExtractor : ExpressionVisitor
{
  private readonly List<string> _properties = new();
  private Type? _declaringType;

  public List<string> Extract<TEntity, TResult>(Expression<Func<TEntity, TResult>> expression)
  {
    _properties.Clear();
    _declaringType = typeof(TEntity);

    Visit(expression);

    return _properties.Distinct().ToList();
  }
  
  protected override Expression VisitMember(MemberExpression m)
  {
    if (m.Expression?.NodeType == ExpressionType.Parameter && m.Member.DeclaringType == _declaringType)
      _properties.Add(m.Member.Name);

    return m;
  }
}