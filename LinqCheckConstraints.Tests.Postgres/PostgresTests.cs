using System;

namespace LinqCheckConstraints.Tests.Postgres;

public class PostgresTestContextTests : TestContextTests
{
  public override TestContext Context => new PostgresTestContextFactory().CreateDbContext(Array.Empty<string>());
}
