using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using _2TDSPR25;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace TodoAppTest;

public class TodoAppTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TodoAppTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllTodos_ShouldReturn200OkWithAList()
    {
        // Act
        var response = await _client.GetAsync("/todoitems");

        //Assert
        response.EnsureSuccessStatusCode(); //Verifica se o response retornou 200˜299
        var todos = await response.Content.ReadFromJsonAsync<List<Todo>>();

        Assert.IsType<List<Todo>>(todos);
    }

    [Fact]
    public async Task _PostTodo_WithoutIdempotencyKey_ShouldReturn400BadRequest()
    {
        //Arrange
        var todo = new TodoItemDTO()
        {
            Name = "Varrer a casa",
            IsComplete = false,
            DayOfWeekToComplete = DayOfWeekAsString.Monday
        };

        //Act
        var response = await _client.PostAsJsonAsync("/todoitems", todo);

        //Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("The Idempotency header key is not found", content);
    }
    
    [Fact]
    public async Task PostTodo_WithIdempotencyKey_ShouldReturn201Created()
    {
        //Arrange
        var todo = new TodoItemDTO()
        {
            Name = "Varrer a casa",
            IsComplete = false,
            DayOfWeekToComplete = DayOfWeekAsString.Monday
        };
        
        _client.DefaultRequestHeaders.Add("IdempotencyKey", "12345");

        //Act
        var response = await _client.PostAsJsonAsync("/todoitems", todo);

        //Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Theory]
    [InlineData(1, HttpStatusCode.OK)]
    [InlineData(-9999, HttpStatusCode.NotFound)]
    public async Task GetTodoById_ShouldReturnCorrectStatusCode(int id, HttpStatusCode statusCode)
    {
        //Act
        var response = await _client.GetAsync($"/todoitems/{id}");
        
        //Assert
        Assert.Equal(statusCode, response.StatusCode);
    }

    [Fact]
    public async Task GetAllTodos_MultipleTimes_ShouldTurnOnRateLimitWith429()
    {
        //Act
        var response = await _client.GetAsync("/todoitems");
        foreach (var _ in Enumerable.Range(0,100))
        {
            response = await _client.GetAsync("/todoitems");
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
                break;
        }
        
        //Assert
        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
        
        Thread.Sleep(TimeSpan.FromSeconds(70));
        
        response = await _client.GetAsync("/todoitems");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}