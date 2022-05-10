using Finaps.LinqCheckConstraints.Core;
using Microsoft.EntityFrameworkCore;

namespace LinqCheckConstraints.Tests;

public class TestContext : DbContext
{
  public const string ByteSmallerThanFive = "ByteSmallerThanFive";
  
  private readonly SqlExpressionConverter _converter;
  protected TestContext(DbContextOptions<TestContext> options, SqlExpressionConverter converter) : base(options)
  {
    _converter = converter;
  }

  protected override void OnModelCreating(ModelBuilder builder)
  {
    builder.Entity<TestEntity>()
      .HasCheckConstraint(_converter, ByteSmallerThanFive, x => x.Byte < 5);
  }
}
