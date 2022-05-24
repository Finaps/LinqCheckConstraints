using System.Text;
using System.Text.Json;

namespace LinqCheckConstraints.Tests.AspNetCore;

public static class TestExtensions
{
  public static HttpContent AsHttpContent<TDto>(this TDto dto) =>
    new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");

  public static async Task<TDto?> AsDto<TDto>(this HttpResponseMessage response) =>
    JsonSerializer.Deserialize<TDto>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
}