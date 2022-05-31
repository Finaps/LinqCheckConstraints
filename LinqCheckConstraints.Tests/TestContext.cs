using System;
using System.Text.RegularExpressions;
using Finaps.LinqCheckConstraints.Core;
using Microsoft.EntityFrameworkCore;

namespace LinqCheckConstraints.Tests;

public class TestContext : DbContext
{
  public const string SbyteGreaterThanMinusFour = "SbyteGreaterThanMinusFour";
  public const string ByteLessThanFive = "ByteSmallerThanFive";
  public const string UshortLessThanOrEqualToTen = "UshortLessThanOrEqualToTen";
  public const string ShortGreaterThanOrEqualToMinusNine = "ShortLessThanOrEqualMinusNine";
  public const string UintEqualToHundred = "UintEqualToHundred";
  public const string IntNotEqualToHundred = "IntNotEqualToHundred";
  public const string TrueEqualToTrue = "TrueEqualToTrue";
  public const string FalseEqualToFalse = "FalseEqualToFalse";
  public const string BoolEqualToTrue = "BoolEqualToTrue";
  public const string UlongGreaterThanLong = "UlongGreaterThanLong";
  public const string DecimalNonNegative = "DecimalNonNegative";
  public const string StringNotEqualToNull = "StringNotEqualToNull";
  public const string NullStringEqualToNull = "NullStringEqualToNull";
  public const string TestEnumEqualToA = "TestEnumEqualToA";
  public const string TestFlagEnumContainsB = "TestFlagEnumContainsB";
  public const string DateTimeGreaterThanEpoch = "DateTimeGreaterThanEpoch";
  public const string DateTimeOffsetSmallerThanNextYear = "DateTimeOffsetSmallerThanNextYear";
  public const string IdNotEqualToEmpty = "IdNotEqualToEmpty";
  public const string EmailIsValid = "EmailIsValid";
  public const string UniqueIsUnique = "UniqueIsUnique";
  public const string DecimalSmallerThanUint = "DecimalSmallerThanUint";
  public const string ComplicatedConstraint = "ComplicatedConstraint";
  public const string TernaryConstraint = "TernaryConstraint";
  public const string SecondTernaryConstraint = "SecondTernaryConstraint";

  private readonly SqlExpressionConverter _converter;
  protected TestContext(DbContextOptions<TestContext> options, SqlExpressionConverter converter) : base(options)
  {
    _converter = converter;
  }

  protected override void OnModelCreating(ModelBuilder builder)
  {
    var nextYear = DateTimeOffset.Now.AddYears(1);

    var email = new Regex(@"^[\w\-\.]+@([\w\-]+\.)+[\w\-]{2,4}$");

    builder.Entity<TestEntity>()
      .HasLinqCheckConstraint(_converter, SbyteGreaterThanMinusFour, x => x.Sbyte > -4)
      .HasLinqCheckConstraint(_converter, ByteLessThanFive, x => x.Byte < 5)
      .HasLinqCheckConstraint(_converter, UshortLessThanOrEqualToTen, x => x.Ushort <= 10)
      .HasLinqCheckConstraint(_converter, ShortGreaterThanOrEqualToMinusNine, x => x.Short >= -9)
      .HasLinqCheckConstraint(_converter, UintEqualToHundred, x => x.Uint == 100)
      .HasLinqCheckConstraint(_converter, IntNotEqualToHundred, x => x.Int != 100)
      .HasLinqCheckConstraint(_converter, TrueEqualToTrue, x => x.True == true)
      .HasLinqCheckConstraint(_converter, FalseEqualToFalse, x => x.False == false)
      .HasLinqCheckConstraint(_converter, BoolEqualToTrue, x => x.Bool)
      .HasLinqCheckConstraint(_converter, UlongGreaterThanLong, x => (long)x.Ulong >= x.Long)
      .HasLinqCheckConstraint(_converter, DecimalNonNegative, x => x.Decimal >= 0)
      .HasLinqCheckConstraint(_converter, StringNotEqualToNull, x => x.String != null)
      .HasLinqCheckConstraint(_converter, NullStringEqualToNull, x => x.NullString == null)
      .HasLinqCheckConstraint(_converter, TestEnumEqualToA, x => x.TestEnum == TestEnum.A)
      .HasLinqCheckConstraint(_converter, TestFlagEnumContainsB, x => (x.TestFlagEnum & TestFlagEnum.B) == TestFlagEnum.B)
      .HasLinqCheckConstraint(_converter, DateTimeGreaterThanEpoch, x => x.DateTime > DateTime.MinValue)
      .HasLinqCheckConstraint(_converter, DateTimeOffsetSmallerThanNextYear, x => x.DateTimeOffset < nextYear)
      .HasLinqCheckConstraint(_converter, IdNotEqualToEmpty, x => x.Id != Guid.Empty)
      .HasLinqCheckConstraint(_converter, EmailIsValid, x => email.IsMatch(x.Email))
      .HasLinqCheckConstraint(_converter, DecimalSmallerThanUint, x => x.Decimal < x.Uint)
      .HasLinqCheckConstraint(_converter, ComplicatedConstraint, x => !x.EnableComplicatedConstraint || (x.Decimal > x.Int && (x.Decimal < x.Uint || email.IsMatch(x.Email))))
      .HasLinqCheckConstraint(_converter, TernaryConstraint, x => (x.Int > 200 ? x.Byte : x.Sbyte) == 0)
      .HasLinqCheckConstraint(_converter, SecondTernaryConstraint, x => x.Uint > 200 ? x.Long == 6 : x.Ulong == 0)
      .HasLinqUniqueConstraint(UniqueIsUnique, x => x.Unique);

    // Seed Entity for Unique Constraints Test
    builder.Entity<TestEntity>()
      .HasData(new TestEntity { Unique = Guid.Empty });
  }
}
