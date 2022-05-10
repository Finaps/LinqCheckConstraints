using System.Threading.Tasks;
using Finaps.LinqCheckConstraints.Core;
using Xunit;

namespace LinqCheckConstraints.Tests;

public abstract class Tests
{
  public abstract TestContext Context { get; }

  [Fact]
  public async Task Byte_Throws_CheckConstraintException_When_GreaterOrEqualTo_Five()
  {
    var context = Context;
    
    context.Add(new TestEntity { Byte = 10 });

    var exception = await Assert.ThrowsAsync<CheckConstraintException>(async () => 
      await context.SaveChangesAsync());

    Assert.Contains(TestContext.ByteSmallerThanFive, exception.Message);
    Assert.Contains(nameof(TestEntity), exception.Message);
  }
}