using System.ComponentModel;
using IdempotentAPI.MinimalAPI;
using Microsoft.EntityFrameworkCore;

namespace _2TDSPR25.Endpoints;

public static class TodoItemsEndpoints
{
    public static void RegisterTodoItemsEndpoints(this WebApplication app)
    {
        #region Routes

        app.MapGet("/", () => "Hello World!")
            .WithTags("teste")
            .ExcludeFromDescription();

        var todoItems = app.MapGroup("/todoitems").WithTags("tarefas");

        todoItems.MapGet("/", GetAllTodos)
            .WithName("GetAllTodos")
            .WithSummary("Apresenta todas as tarefas")
            .WithDescription("""
                             Apresenta todas as tarefas cadastradas no sistema, incluindo as completas e incompletas.
                             Não existe paginação ou filtros, assim que, se houver muitos registros, 
                             a consulta pode demorar um certo tempo.
                             """)
            .Produces<List<TodoItemDTO>>();
        todoItems.MapGet("/complete", GetCompleteTodos)
            .WithName("GetCompleteTodos")
            .WithSummary("Apresenta todas as tarefas completadas")
            .WithDescription("""
                             Apresenta todas as tarefas cadastradas no sistema que estejam completadas.
                             Não existe paginação ou filtros, assim que, se houver muitos registros, 
                             a consulta pode demorar um certo tempo.
                             """)
            .Produces<List<TodoItemDTO>>();
        ;
        todoItems.MapGet("/{id:int}", GetTodo)
            .WithName("GetTodoById")
            .WithSummary("Busca uma tarefa por id")
            .WithDescription("Busca uma tarefa por id. Se a tarefa não for encontrada, " +
                             "retorna um status code 404 (Not Found).")
            .Produces<TodoItemDTO>(200)
            .Produces(404);
        todoItems.MapPost("/", async (TodoItemDTO todoItemDTO) =>
            {
                using var scope = app.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<TodoDb>();

                var todoItem = new Todo
                {
                    IsComplete = todoItemDTO.IsComplete,
                    Name = todoItemDTO.Name
                };

                db.Todos.Add(todoItem);
                await db.SaveChangesAsync();

                todoItemDTO = new TodoItemDTO(todoItem);

                return TypedResults.Created($"/todoitems/{todoItem.Id}", todoItemDTO);
            })
            .WithName("CreateTodo")
            .WithSummary("Cria uma nova tarefa")
            .WithDescription("Cria uma nova tarefa. O id da tarefa é gerado automaticamente pelo sistema")
            .Produces<TodoItemDTO>(201)
            .Produces(400)
            .AddEndpointFilter<IdempotentAPIEndpointFilter>()
            .Accepts<TodoItemDTO>("application/json", "application/xml");
        todoItems.MapPut("/{id}", UpdateTodo);
        todoItems.MapDelete("/{id}", DeleteTodo)
            .WithName("DeleteTodoById")
            .WithSummary("Delete uma tarefa pelo seu id")
            .WithDescription("Deleta uma tarefa pelo seu id. Se a tarefa não for encontrada, " +
                             "retorna um status code 404 (Not Found).")
            .Produces(204)
            .Produces(400);
//.Accepts<TodoItemDTO>("application/json", "application/xml");;

        app.Run();

        #endregion
    }
    
    #region Services
    
    static async Task<IResult> GetAllTodos(TodoDb db)
    {
        return TypedResults.Ok(await db.Todos.Select(x => new TodoItemDTO(x)).ToArrayAsync());
    }

    static async Task<IResult> GetCompleteTodos(TodoDb db) {
        return TypedResults.Ok(await db.Todos.Where(t => t.IsComplete).Select(x => new TodoItemDTO(x)).ToListAsync());
    }

    static async Task<IResult> GetTodo([Description("id da tarefa que será buscada.")]int id, TodoDb db)
    {
        return await db.Todos.FindAsync(id)
            is Todo todo
            ? TypedResults.Ok(new TodoItemDTO(todo))
            : TypedResults.NotFound();
    }

    static async Task<IResult> CreateTodo([Description("Tarefa a ser cadastrada")]TodoItemDTO todoItemDTO, TodoDb db)
    {
        var todoItem = new Todo
        {
            IsComplete = todoItemDTO.IsComplete,
            Name = todoItemDTO.Name
        };

        db.Todos.Add(todoItem);
        await db.SaveChangesAsync();

        todoItemDTO = new TodoItemDTO(todoItem);

        return TypedResults.Created($"/todoitems/{todoItem.Id}", todoItemDTO);
    }

    static async Task<IResult> UpdateTodo([Description("id da tarefa que será buscada.")]int id, TodoItemDTO todoItemDTO, TodoDb db)
    {
        var todo = await db.Todos.FindAsync(id);

        if (todo is null) return TypedResults.NotFound();

        todo.Name = todoItemDTO.Name;
        todo.IsComplete = todoItemDTO.IsComplete;

        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    }

    static async Task<IResult> DeleteTodo([Description("id da tarefa que será buscada.")]int id, TodoDb db)
    {
        if (await db.Todos.FindAsync(id) is Todo todo)
        {
            db.Todos.Remove(todo);
            await db.SaveChangesAsync();
            return TypedResults.NoContent();
        }

        return TypedResults.NotFound();
    }
    #endregion
}