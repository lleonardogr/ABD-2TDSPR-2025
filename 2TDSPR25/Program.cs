using _2TDSPR25;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDb>(
    options => options.UseInMemoryDatabase("TodoDb"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

var todogroup = app.MapGroup("/todoitems");

todogroup.MapGet("/", async (TodoDb db) => await db.Todos.ToListAsync());

todogroup.MapGet("/complete", async (TodoDb db) =>
    await db.Todos.Where(t => t.IsComplete).ToListAsync());

todogroup.MapGet("/{id}", async (int id, TodoDb db) =>
    await db.Todos.FindAsync(id) is Todo todo 
        ? Results.Ok(todo) : 
        Results.NotFound()
);

todogroup.MapPost("/", async (Todo todo, TodoDb db) =>
{
    db.Todos.Add(todo);
    await db.SaveChangesAsync();
    return Results.Created($"/todoitems/{todo.Id}", todo);
});

todogroup.MapPut("/todoitems/{id}", async (int id, Todo todo, TodoDb db) =>
{
    var todo = db.Todos.
});

todogroup.MapDelete("/{id}", async (int id, TodoDb db) =>
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        
        return Results.NoContent();
    }
    
    return Results.NotFound();
});

app.Run();