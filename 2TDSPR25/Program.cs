using System.ComponentModel;
using System.Threading.RateLimiting;
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

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 10,
                QueueLimit = 0,
                Window = TimeSpan.FromSeconds(60)
            }));

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "text/plain";
        context.HttpContext.Response.Headers.RetryAfter = "60";
        await context.HttpContext.Response.WriteAsync("Muitas requisições. Por favor, tente novamente mais tarde.", 
            cancellationToken);
    };
});

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
            Description = """
                          API para gerenciamento de tarefas",
                          
                          Boa vizinhança:
                          
                          Recomendamos que seja feita apenas 10 requisições por segundo no máximo, 
                          caso haja exageros de uso, você receberá o status code 429 (Too Many Requests).
                          As consultas são monitoradas por IP ou pelo header User-Agent, caso persista o abuso da api, seu IP ou app pode ser bloqueado.
                          """,
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

app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.RegisterTodoItemsEndpoints();
