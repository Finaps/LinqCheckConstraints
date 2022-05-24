using System.Net;
using LinqCheckConstraints.Tests.AspNetCore;
using LinqCheckConstraints.Tests.Postgres.AspNetCore.Database;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace LinqCheckConstraints.Tests.Postgres.AspNetCore;

public class AspNetCoreTests
{
  [Fact]
  public async Task Check_Constraint_Validation_Returns_ValidationResponse()
  {
    var response = await Client.PostAsync("test", new TestEntity
    {
      String = "Hello World",
      Int = 15
    }.AsHttpContent());

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

    var result = await response.AsDto<ValidationError>();
    
    Assert.NotNull(result);
    Assert.Contains(nameof(TestEntity.Int), result!.Errors.Keys);
  }

  private static readonly HttpClient Client = new WebApplicationFactory<Program>().WithWebHostBuilder(x =>
      x.ConfigureAppConfiguration(builder => builder.AddJsonFile("appsettings.json", false))).CreateClient();
}