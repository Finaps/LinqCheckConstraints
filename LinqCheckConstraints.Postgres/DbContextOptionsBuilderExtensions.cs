using Microsoft.EntityFrameworkCore;

namespace Finaps.LinqCheckConstraints.Postgres;

public static class DbContextOptionsBuilderExtensions
{
  public static DbContextOptionsBuilder AddLinqCheckConstraintExceptions(this DbContextOptionsBuilder builder) =>
    builder.AddInterceptors(new PostgresConstraintInterceptor());
}