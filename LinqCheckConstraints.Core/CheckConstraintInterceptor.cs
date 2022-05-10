using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Finaps.LinqCheckConstraints.Core;

public abstract class CheckConstraintInterceptor<T> : SaveChangesInterceptor where T : DbException
{
  protected abstract string? GetViolatedCheckConstraintName(T exception);
  
  public override void SaveChangesFailed(DbContextErrorEventData eventData)
  {
    MaybeThrowCheckConstraintException(eventData);
    
    base.SaveChangesFailed(eventData);
  }

  public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
  {
    MaybeThrowCheckConstraintException(eventData);
    
    return base.SaveChangesFailedAsync(eventData, cancellationToken);
  }

  private void MaybeThrowCheckConstraintException(IErrorEventData eventData)
  {
    if (eventData.Exception is not DbUpdateException exception) return;
    if (eventData.Exception.GetBaseException() is not T providerException) return;

    var constraint = GetViolatedCheckConstraintName(providerException);

    if (constraint == null || !CheckConstraintExceptionCache.Messages.TryGetValue(constraint, out var message)) return;

    throw new CheckConstraintException(message, exception);
  }
}