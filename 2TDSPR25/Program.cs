using System.ComponentModel;
using _2TDSPR25;
using _2TDSPR25.Endpoints;
using IdempotentAPI.Cache.DistributedCache.Extensions.DependencyInjection;
using IdempotentAPI.Core;
using IdempotentAPI.Extensions.DependencyInjection;
using IdempotentAPI.MinimalAPI;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

#region Database
builder.Services.AddDbContext<TodoDb>(
    options => options.UseInMemoryDatabase("TodoDb"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
#endregion

#region Idempotency
builder.Services.AddIdempotentAPI();
builder.Services.AddIdempotentMinimalAPI(new IdempotencyOptions());
builder.Services.AddDistributedMemoryCache();
builder.Services.AddIdempotentAPIUsingDistributedCache();
#endregion

#region OpenAPI / Swagger / Scalar
builder.Services.AddOpenApi(options =>
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new OpenApiInfo()
        {
            Title = "API de Tarefas",
            Version = "1.0.0",
            Description = "API para gerenciamento de tarefas",
            License = new OpenApiLicense()
            {
               Name  = "MIT",
               Url = new Uri("https://opensource.org/license/mit/")
            },
            Contact = new OpenApiContact()
            {
                Email = "teste@fiap.com.br",
                Name = "FIAP",
                Url = new Uri("https://www.fiap.com.br")
            }
        };
        return Task.CompletedTask;
    });
});
#endregion

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.RegisterTodoItemsEndpoints();
