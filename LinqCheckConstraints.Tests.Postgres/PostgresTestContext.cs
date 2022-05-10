using System;
using Finaps.LinqCheckConstraints.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace LinqCheckConstraints.Tests.Postgres;

public class PostgresTestContext : TestContext
{
  public PostgresTestContext(DbContextOptions<TestContext> options) : base(options, new PostgresSqlExpressionConverter()) {}
  
  protected override void OnConfiguring(DbContextOptionsBuilder builder)
  {
    base.OnConfiguring(builder);
    builder.AddLinqCheckConstraintExceptions();
  }
}

public class PostgresTestContextFactory : IDesignTimeDbContextFactory<PostgresTestContext>
{
  public PostgresTestContext CreateDbContext(string[] args)
  {
    var configuration = new ConfigurationBuilder()
      .AddJsonFile("appsettings.json", false)
      .AddEnvironmentVariables()
      .Build();
    
    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

    return new PostgresTestContext(new DbContextOptionsBuilder<TestContext>()
      .UseNpgsql(configuration.GetConnectionString("TestContext"))
      .EnableSensitiveDataLogging()
      .Options);
  }
}