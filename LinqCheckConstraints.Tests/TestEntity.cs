using System;

namespace LinqCheckConstraints.Tests;

public class TestEntity
{
  public Guid Id { get; set; }
  
  public sbyte Sbyte { get; set; }
  public byte Byte { get; set; }
  
  public ushort Ushort { get; set; }
  public short Short { get; set; }
  
  public uint Uint { get; set; }
  public int Int { get; set; }
  
  public ulong Ulong { get; set; }
  public long Long { get; set; }

  public string String { get; set; } = "";
  
  public DateTime DateTime { get; set; }
  public DateTimeOffset DateTimeOffset { get; set; }
}