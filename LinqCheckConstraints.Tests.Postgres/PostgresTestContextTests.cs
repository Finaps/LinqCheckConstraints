namespace LinqCheckConstraints.Tests.Postgres;

public class PostgresTestContextTests : TestContextTests
{
  public PostgresTestContextTests() : base(new PostgresTestContextFactory().CreateDbContext(new string[] { })) { }
}
