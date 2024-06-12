using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TodoApp.Contexts;
using TodoApp.Models.Entities.Concretes;

namespace TodoApp.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TodoController : ControllerBase
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public TodoController(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        using var context = _contextFactory.CreateDbContext();
        var todos = await context.TodoItems.Where(p => p.UserId == userId).ToListAsync();
        return Ok(todos);
    }

    [HttpGet("{id}")]

    public async Task<IActionResult>Get(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        using var context = _contextFactory.CreateDbContext();
        var todo = await context.TodoItems.FirstOrDefaultAsync(todo => todo.Id == id && todo.UserId == userId);

        if (todo == null)
        {
            return NotFound();
        }

        return Ok(todo);
    }

    [HttpPost]

    public async Task<IActionResult> Create([FromBody]TodoItem item)
    {
        if(!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        item.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        using var context = _contextFactory.CreateDbContext();
        context.TodoItems.Add(item);
        await context.SaveChangesAsync();
        return Ok();
    }

}
