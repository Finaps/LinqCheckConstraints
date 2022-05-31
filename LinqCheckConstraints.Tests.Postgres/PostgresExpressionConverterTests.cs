using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Finaps.LinqCheckConstraints.Postgres;
using Xunit;

namespace LinqCheckConstraints.Tests.Postgres;

public class PostgresExpressionConverterTests
{
  public class BooleanEntity
  {
    public bool A { get; set; }
    public bool B { get; set; }
    public bool C { get; set; }
    public bool D { get; set; }
  }

  public class NumericEntity
  {
    public int A { get; set; }
    public int B { get; set; }
    public int C { get; set; }
    public int D { get; set; }
  }

  public class StringEntity
  {
    public string A { get; set; }
    public string B { get; set; }
    public string C { get; set; }
    public string D { get; set; }
  }

  public static TheoryData<Expression<Func<BooleanEntity, bool>>, string> BooleanData => new()
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

  public static TheoryData<Expression<Func<NumericEntity, bool>>, string> NumericData => new()
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
    { x => x.A > x.B ? x.C > 0 : x.D > 0, "CASE WHEN \"A\" > \"B\" THEN \"C\" > 0 ELSE \"D\" > 0 END" },
    { x => -(x.A + x.B) > x.C, "- (\"A\" + \"B\") > \"C\"" },
    { x => -x.A + x.B > x.C, "- \"A\" + \"B\" > \"C\"" },
    { x => - (-x.A + x.B) > x.C, "- (- \"A\" + \"B\") > \"C\"" },
  };

  public static TheoryData<Expression<Func<StringEntity, bool>>, string> StringData => new()
  {
    { x => x.A + x.B + x.C + 42 == "Hello World", "\"A\" || \"B\" || \"C\" || 42 = 'Hello World'"},
    { x => Regex.IsMatch(x.A, ".*"), "\"A\" ~ '.*'" },
    { x => new Regex(".*").IsMatch(x.A), "\"A\" ~ '.*'" },
    { x => Regex.IsMatch(x.A + x.B, ".*" + "$"), "\"A\" || \"B\" ~ '.*$'" },
    { x => new Regex(".*" + "$").IsMatch(x.A + x.B), "\"A\" || \"B\" ~ '.*$'" },
    { x => x.A.ToLower() == "", "lower(\"A\") = ''"},
    { x => x.A.ToUpper() == "", "upper(\"A\") = ''"},
    { x => x.A.Length == 4, "char_length(\"A\") = 4"},
    { x => "Hello World".Length == 4, "char_length('Hello World') = 4"},
    { x => (x.A + x.B).Length < 5, "char_length(\"A\" || \"B\") < 5"}
  };

  [Theory]
  [MemberData(nameof(BooleanData))]
  public void PostgresSqlExpressionConverter_Converts_Boolean_Logic(Expression<Func<BooleanEntity, bool>> e, string sql) =>
    Assert.Equal(sql, new PostgresSqlExpressionConverter().Convert(e));
  
  [Theory]
  [MemberData(nameof(NumericData))]
  public void PostgresSqlExpressionConverter_Converts_Numeric_Expressions(Expression<Func<NumericEntity, bool>> e, string sql) =>
    Assert.Equal(sql, new PostgresSqlExpressionConverter().Convert(e));
  
  [Theory]
  [MemberData(nameof(StringData))]
  public void PostgresSqlExpressionConverter_Converts_String_Expressions(Expression<Func<StringEntity, bool>> e, string sql) =>
    Assert.Equal(sql, new PostgresSqlExpressionConverter().Convert(e));
}