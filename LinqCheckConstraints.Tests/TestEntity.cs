using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinqCheckConstraints.Tests;

public enum TestEnum { A = 1, B = 2, C = 3 }

[Flags] public enum TestFlagEnum { A = 1, B = 2, C = 4}

public class TestEntity
{
  [DatabaseGenerated(DatabaseGeneratedOption.None)]
  public Guid Id { get; set; } = Guid.NewGuid();

  public bool Bool { get; set; } = true;
  public bool True { get; set; } = true;
  public bool False { get; set; } = false;
  
  public sbyte Sbyte { get; set; }
  public byte Byte { get; set; }
  
  public ushort Ushort { get; set; }
  public short Short { get; set; }

  public uint Uint { get; set; } = 100;
  public int Int { get; set; }
  
  public ulong Ulong { get; set; }
  public long Long { get; set; }
  
  public decimal Decimal { get; set; }

  public TestEnum TestEnum { get; set; } = TestEnum.A;
  public TestFlagEnum TestFlagEnum { get; set; } = TestFlagEnum.B | TestFlagEnum.C;

  public string? String { get; set; } = "";
  public string? NullString { get; set; }

  public DateTime DateTime { get; set; } = DateTime.Now;
  public DateTimeOffset DateTimeOffset { get; set; } = DateTime.Now;

  public string Email { get; set; } = "test@examle.com";
  public Guid Unique { get; set; } = Guid.NewGuid();
  public Guid AlsoUnique { get; set; } = Guid.NewGuid();
}
