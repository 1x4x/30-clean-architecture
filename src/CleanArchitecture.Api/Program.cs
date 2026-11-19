using CleanArchitecture.Application;
using CleanArchitecture.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
builder.Services.AddScoped<CreateOrder>();
builder.Services.AddScoped<ConfirmOrder>();

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { message = "Clean Architecture demo", endpoints = new[] { "POST /orders", "POST /orders/{id}/confirm", "GET /orders/{id}" } }));

app.MapPost("/orders", async (CreateOrder useCase, CreateOrderRequest request, CancellationToken ct) =>
{
    try
    {
        var id = await useCase.ExecuteAsync(request.Sku, request.UnitPrice, request.Quantity, ct);
        return Results.Created($"/orders/{id}", new { id });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/orders/{id:guid}/confirm", async (Guid id, ConfirmOrder useCase, CancellationToken ct) =>
{
    try
    {
        await useCase.ExecuteAsync(id, ct);
        return Results.NoContent();
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound();
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapGet("/orders/{id:guid}", async (Guid id, IOrderRepository repository, CancellationToken ct) =>
{
    var order = await repository.GetAsync(id, ct);
    return order is null
        ? Results.NotFound()
        : Results.Ok(new { order.Id, order.IsConfirmed, order.Total, order.Items });
});

app.Run();

public sealed record CreateOrderRequest(string Sku, decimal UnitPrice, int Quantity);
