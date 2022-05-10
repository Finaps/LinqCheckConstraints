using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finaps.LinqCheckConstraints.Core;

public static class EntityTypeBuilderExtensions
{
  public static EntityTypeBuilder<TEntity> HasCheckConstraint<TEntity>(
    this EntityTypeBuilder<TEntity> builder, SqlExpressionConverter converter, string name,
    Expression<Func<TEntity, bool>> expression, string? message = null) where TEntity : class
  {
    var constraintName = $"CK_{typeof(TEntity).Name}_{name}";

    CheckConstraintExceptionCache.Messages[constraintName] = 
      message ?? $"Check constraint '{name}' violated while updating entry of type '{typeof(TEntity).Name}'.";

    return builder.HasCheckConstraint(constraintName, converter.Convert(expression));
  }
}