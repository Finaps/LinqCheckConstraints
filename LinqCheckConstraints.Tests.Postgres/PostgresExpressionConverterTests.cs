using System;
using System.Linq.Expressions;
using Finaps.LinqCheckConstraints.Postgres;
using Xunit;

namespace LinqCheckConstraints.Tests.Postgres;

public class PostgresExpressionConverterTests
{
  public class Boolean
  {
    public bool A { get; set; }
    public bool B { get; set; }
    public bool C { get; set; }
    public bool D { get; set; }
  }

  public class Numeric
  {
    public int A { get; set; }
    public int B { get; set; }
    public int C { get; set; }
    public int D { get; set; }
  }

  public static TheoryData<Expression<Func<Boolean, bool>>, string> BooleanData => new()
  {
    { x => x.A, "\"A\"" },
    { x => x.A == true, "\"A\" = True" },
    { x => x.A == false, "\"A\" = False" },
    { x => x.A == x.B, "\"A\" = \"B\"" },
    { x => x.A != x.B, "\"A\" <> \"B\"" },
    { x => !x.A && !x.B, "NOT \"A\" AND NOT \"B\"" },
    { x => (x.A || x.B) && x.C, "(\"A\" OR \"B\") AND \"C\"" },
    { x => x.A || x.B && x.C, "\"A\" OR \"B\" AND \"C\"" },
    { x => x.A || (x.B && x.C), "\"A\" OR \"B\" AND \"C\"" },
    { x => x.A || x.B || x.C || x.D, "\"A\" OR \"B\" OR \"C\" OR \"D\"" },
    { x => x.A || x.B && x.C || x.D, "\"A\" OR \"B\" AND \"C\" OR \"D\"" },
    { x => (x.A || x.B) && (x.C || x.D), "(\"A\" OR \"B\") AND (\"C\" OR \"D\")" },
  };

  public static TheoryData<Expression<Func<Numeric, bool>>, string> NumericData => new()
  {
    { x => x.A == 0, "\"A\" = 0" },
    { x => x.A * x.B + x.C == 0, "\"A\" * \"B\" + \"C\" = 0"},
    { x => x.A * (x.B + x.C) == 0, "\"A\" * (\"B\" + \"C\") = 0"},
    { x => x.A / x.B - x.C == 0, "\"A\" / \"B\" - \"C\" = 0"},
    { x => x.A * x.B % x.C == 0, "\"A\" * \"B\" % \"C\" = 0"},
    { x => (x.A * x.B) % x.C == 0, "\"A\" * \"B\" % \"C\" = 0"},
    { x => x.A * (x.B % x.C) == 0, "\"A\" * (\"B\" % \"C\") = 0"},
    { x => x.A % x.B * x.C == 0, "\"A\" % \"B\" * \"C\" = 0"},
    { x => (x.A % x.B) * x.C == 0, "\"A\" % \"B\" * \"C\" = 0"},
    { x => x.A % (x.B * x.C) == 0, "\"A\" % (\"B\" * \"C\") = 0"},
    { x => (x.A + x.B - x.C) * x.D == 0, "(\"A\" + \"B\" - \"C\") * \"D\" = 0" },
    { x => (x.A > x.B ? x.C : x.D) > 0, "CASE WHEN \"A\" > \"B\" THEN \"C\" ELSE \"D\" END > 0" },
    { x => x.A > x.B ? x.C > 0 : x.D > 0, "CASE WHEN \"A\" > \"B\" THEN \"C\" > 0 ELSE \"D\" > 0 END" }
  };

  [Theory]
  [MemberData(nameof(BooleanData))]
  public void PostgresSqlExpressionConverter_Converts_Boolean_Logic(Expression<Func<Boolean, bool>> e, string sql) =>
    Assert.Equal(sql, new PostgresSqlExpressionConverter().Convert(e));
  
  [Theory]
  [MemberData(nameof(NumericData))]
  public void PostgresSqlExpressionConverter_Converts_Numeric_Expressions(Expression<Func<Numeric, bool>> e, string sql) =>
    Assert.Equal(sql, new PostgresSqlExpressionConverter().Convert(e));
}