using Finaps.LinqCheckConstraints.Postgres;
using LinqCheckConstraints.Tests.AspNetCore;
using LinqCheckConstraints.Tests.Postgres.AspNetCore.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options => options.Filters.Add(new ExceptionFilter()));

builder.Services.AddDbContext<AspNetCoreTestContext>(options => options
  .UseNpgsql(builder.Configuration.GetConnectionString("AspNetCoreTestContext"))
  .AddLinqCheckConstraintExceptions());

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();