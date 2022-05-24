using System.Net;

namespace LinqCheckConstraints.Tests.AspNetCore;

public class ValidationError
{
  public string Type { get; set; }
  public string Title { get; set; }
  public HttpStatusCode Status { get; set; }
  public string TraceId { get; set; }
  public Dictionary<string, List<string>> Errors { get; set; }
}