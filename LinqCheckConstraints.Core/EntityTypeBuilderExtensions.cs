using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finaps.LinqCheckConstraints.Core;

public static class EntityTypeBuilderExtensions
{
  public static EntityTypeBuilder<TEntity> HasLinqCheckConstraint<TEntity>(
    this EntityTypeBuilder<TEntity> builder, SqlExpressionConverter converter, string name,
    Expression<Func<TEntity, bool>> expression, string? message = null) where TEntity : class
  {
    var type = typeof(TEntity);
    var constraint = $"CK_{type.Name}_{name}";
    
    ConstraintExceptionCache.Add(new ConstraintExceptionInfo
    {
      Type = ConstraintType.Check,
      Name = name,
      ConstraintName = constraint,
      EntityType = type,
      Message = message ?? $"Check constraint '{name}' violated while updating entry of type '{type.Name}'.",
      Properties = new ExpressionPropertiesExtractor().Extract(expression)
    });

    return builder.HasCheckConstraint(constraint, converter.Convert(expression));
  }

  public static EntityTypeBuilder<TEntity> HasLinqUniqueConstraint<TEntity>(this EntityTypeBuilder<TEntity> builder, string name, Expression<Func<TEntity, object?>> expression, string? message = null) where TEntity : class
  {
    var type = typeof(TEntity);
    var constraint = $"IX_{type.Name}_{name}";
    
    ConstraintExceptionCache.Add(new ConstraintExceptionInfo
    {
      Type = ConstraintType.Unique,
      Name = name,
      ConstraintName = constraint,
      EntityType = type,
      Message = message ?? $"Check constraint '{name}' violated while updating entry of type '{type.Name}'.",
      Properties = new ExpressionPropertiesExtractor().Extract(expression)
    });
    
    builder.HasIndex(expression, constraint).IsUnique();

    return builder;
  }
}