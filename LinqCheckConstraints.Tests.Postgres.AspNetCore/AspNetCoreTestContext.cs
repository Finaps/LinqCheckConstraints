using Finaps.LinqCheckConstraints.Postgres;
using Microsoft.EntityFrameworkCore;

namespace LinqCheckConstraints.Tests.Postgres.AspNetCore.Database;

public class TestEntity
{
  public Guid Id { get; set; }
  public string String { get; set; }
  public int Int { get; set; }
  public bool Bool { get; set; }
}

public class AspNetCoreTestContext : DbContext
{
  public AspNetCoreTestContext(DbContextOptions<AspNetCoreTestContext> options) : base(options) { }
  
  protected override void OnModelCreating(ModelBuilder builder)
  {
    builder.Entity<TestEntity>()
      .HasLinqCheckConstraint("IntLessThanTen", x => x.Int < 10);
  }
}