using Microsoft.EntityFrameworkCore;

namespace Finaps.LinqCheckConstraints.Core;

public class CheckConstraintException : DbUpdateException
{
  public CheckConstraintException(string message, Exception? innerException) : base(message, innerException) { }
}