using System.Data.Common;

namespace Finaps.LinqCheckConstraints.Core;

public enum ConstraintType
{
  Check = 0,
  Unique = 1,
}

public class ConstraintExceptionInfo
{
  public ConstraintType Type { get; set; }
  public string Name { get; set; }
  public string ConstraintName { get; init; }
  public Type EntityType { get; init; }
  public string Message { get; init; }
  public List<string> Properties { get; init; }
}

public class ConstraintException : DbException
{
  public string Name { get; }
  public string ConstraintName { get; }
  public Type EntityType { get; }
  public List<string> Properties { get; init; }

  public ConstraintException(ConstraintExceptionInfo info, Exception? inner = null) : 
    base(info.Message, inner)
  {
    Name = info.Name;
    ConstraintName = info.ConstraintName;
    EntityType = info.EntityType;
    Properties = info.Properties;
  }
}

internal static class ConstraintExceptionCache
{
  public static Dictionary<string, ConstraintExceptionInfo> Infos { get; } = new();
  public static readonly Dictionary<Type, List<string>> Constraints = new();

  public static ConstraintException? GetException(string? constraint, Exception? inner = null) =>
    !Infos.TryGetValue(constraint ?? "", out var info) ? null : new ConstraintException(info, inner);

  public static void MaybeThrowException(string? constraint, Exception? inner = null)
  {
    var exception = GetException(constraint, inner);
    if (exception != null) throw exception;
  }

  public static void Add(ConstraintExceptionInfo info)
  {
    Infos[info.ConstraintName] = info;
    
    if (!Constraints.ContainsKey(info.EntityType))
      Constraints[info.EntityType] = new List<string>();
    
    Constraints[info.EntityType].Add(info.ConstraintName);
  }
}