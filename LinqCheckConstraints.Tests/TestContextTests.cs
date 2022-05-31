using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Finaps.LinqCheckConstraints.Core;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LinqCheckConstraints.Tests;

public abstract class TestContextTests
{
  protected DbContext Context { get; }

  public TestContextTests(DbContext context) => Context = context;

  [Fact]
  public async Task Can_Insert_Default_TestEntity()
  {
    Context.Add(new TestEntity());
    await Context.SaveChangesAsync();
  }
  
  public static TheoryData<string, TestEntity, List<string>> Data => new()
  {
    { TestContext.SbyteGreaterThanMinusFour, new TestEntity { Sbyte = -8 }, new List<string> { nameof(TestEntity.Sbyte) } },
    { TestContext.ByteLessThanFive, new TestEntity { Byte = 10 }, new List<string> { nameof(TestEntity.Byte) } },
    { TestContext.UshortLessThanOrEqualToTen, new TestEntity { Ushort = 11 }, new List<string> { nameof(TestEntity.Ushort) } },
    { TestContext.ShortGreaterThanOrEqualToMinusNine, new TestEntity { Short = -10 }, new List<string> { nameof(TestEntity.Short) } },
    { TestContext.UintEqualToHundred, new TestEntity { Uint = 99 }, new List<string> { nameof(TestEntity.Uint) } },
    { TestContext.IntNotEqualToHundred, new TestEntity { Int = 100 }, new List<string> { nameof(TestEntity.Int) } },
    { TestContext.TrueEqualToTrue, new TestEntity { True = false }, new List<string> { nameof(TestEntity.True) } },
    { TestContext.FalseEqualToFalse, new TestEntity { False = true }, new List<string> { nameof(TestEntity.False) } },
    { TestContext.BoolEqualToTrue, new TestEntity { Bool = false }, new List<string> { nameof(TestEntity.Bool) } },
    { TestContext.UlongGreaterThanLong, new TestEntity { Long = 20 }, new List<string> { nameof(TestEntity.Ulong), nameof(TestEntity.Long) } },
    { TestContext.DecimalNonNegative, new TestEntity { Decimal = -1 }, new List<string> { nameof(TestEntity.Decimal) } },
    { TestContext.StringNotEqualToNull, new TestEntity { String = null }, new List<string> { nameof(TestEntity.String) } },
    { TestContext.NullStringEqualToNull, new TestEntity { NullString = "Not Null!" }, new List<string> { nameof(TestEntity.NullString) } },
    { TestContext.TestEnumEqualToA, new TestEntity { TestEnum = TestEnum.B }, new List<string> { nameof(TestEntity.TestEnum) } },
    { TestContext.TestFlagEnumContainsB, new TestEntity { TestFlagEnum = TestFlagEnum.A }, new List<string> { nameof(TestEntity.TestFlagEnum) } },
    { TestContext.DateTimeGreaterThanEpoch, new TestEntity { DateTime = DateTime.MinValue }, new List<string> { nameof(TestEntity.DateTime) } },
    { TestContext.DateTimeOffsetSmallerThanNextYear, new TestEntity { DateTimeOffset = DateTimeOffset.Now.AddYears(2) }, new List<string> { nameof(TestEntity.DateTimeOffset) } },
    { TestContext.IdNotEqualToEmpty, new TestEntity { Id = Guid.Empty }, new List<string> { nameof(TestEntity.Id) } },
    { TestContext.EmailIsValid, new TestEntity { Email = "definitely not an email" }, new List<string> { nameof(TestEntity.Email) } },
    { TestContext.UniqueIsUnique, new TestEntity { Unique = Guid.Empty }, new List<string> { nameof(TestEntity.Unique) } },
    { TestContext.DecimalSmallerThanUint, new TestEntity { Decimal = 1000 }, new List<string> { nameof(TestEntity.Decimal), nameof(TestEntity.Uint) } },
    { TestContext.ComplicatedConstraint, new TestEntity { EnableComplicatedConstraint = true, Int = 1000 }, new List<string> { nameof(TestEntity.EnableComplicatedConstraint), nameof(TestEntity.Decimal), nameof(TestEntity.Int), nameof(TestEntity.Uint), nameof(TestEntity.Email) }},
    { TestContext.TernaryConstraint, new TestEntity { Int = 400, Byte = 2 }, new List<string> { nameof(TestEntity.Int), nameof(TestEntity.Byte), nameof(TestEntity.Sbyte)} },
    { TestContext.TernaryConstraint, new TestEntity { Sbyte = 2 }, new List<string> { nameof(TestEntity.Int), nameof(TestEntity.Byte), nameof(TestEntity.Sbyte)} },
    { TestContext.SecondTernaryConstraint, new TestEntity { Uint = 400 }, new List<string> { nameof(TestEntity.Uint), nameof(TestEntity.Long), nameof(TestEntity.Ulong)} },
    { TestContext.SecondTernaryConstraint, new TestEntity { Ulong = 2 }, new List<string> { nameof(TestEntity.Uint), nameof(TestEntity.Long), nameof(TestEntity.Ulong)} },
  };

  [Theory]
  [MemberData(nameof(Data))]
  public async Task Database_Can_Throw_ConstraintExceptions(string constraint, TestEntity entity, List<string> properties)
  {
    var context = Context;

    context.Add(entity);
    
    var exception = await Assert.ThrowsAsync<ConstraintException>(async () => 
      await context.SaveChangesAsync());
    
    Assert.Equal(constraint, exception.Name);
    Assert.Equal(typeof(TestEntity), exception.EntityType);
    Assert.Equal(properties, exception.Properties);
  }
}