using System;

namespace LinqCheckConstraints.Tests.Postgres;

public class PostgresTests : Tests
{
  public override TestContext Context => new PostgresTestContextFactory().CreateDbContext(Array.Empty<string>());
}