using Microsoft.AspNetCore.Mvc;

namespace LinqCheckConstraints.Tests.Postgres.AspNetCore.Database.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
  private readonly AspNetCoreTestContext _context;

  public TestController(AspNetCoreTestContext context)
  {
    _context = context;
  }
  
  [HttpPost]
  [ProducesResponseType(201)]

  public async Task<ActionResult> Create(TestEntity entity, CancellationToken cancellationToken = default)
  {
    var id = _context.Add(entity).Entity.Id;
    await _context.SaveChangesAsync(cancellationToken);
    return CreatedAtAction(nameof(Get), id, id);
  }

  [HttpGet("{id:guid}")]
  [ProducesResponseType(200)]
  [ProducesResponseType(404)]
  public async Task<ActionResult<TestEntity?>> Get(Guid id) => await _context.Set<TestEntity>().FindAsync(id);
}