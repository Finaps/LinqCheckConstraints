using Finaps.LinqCheckConstraints.Core;
using Npgsql;

namespace Finaps.LinqCheckConstraints.Postgres;

public class PostgresCheckConstraintInterceptor : CheckConstraintInterceptor<PostgresException>
{
  protected override string? GetViolatedCheckConstraintName(PostgresException exception) => exception.ConstraintName;
}