using System.Linq.Expressions;
using Finaps.LinqCheckConstraints.Core;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finaps.LinqCheckConstraints.Postgres;

public static class EntityTypeBuilderExtensions
{
  public static EntityTypeBuilder<TEntity> HasCheckConstraint<TEntity>(
    this EntityTypeBuilder<TEntity> builder, string name,
    Expression<Func<TEntity, bool>> expression, string? message = null) where TEntity : class =>
    builder.HasCheckConstraint(new PostgresSqlExpressionConverter(), name, expression, message);
}