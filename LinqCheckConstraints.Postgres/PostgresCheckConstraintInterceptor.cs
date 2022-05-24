using Finaps.LinqCheckConstraints.Core;
using Npgsql;

namespace Finaps.LinqCheckConstraints.Postgres;

public class PostgresConstraintInterceptor : ConstraintInterceptor<PostgresException>
{
  protected override string? GetCheckConstraintName(PostgresException exception) => exception.ConstraintName;
}