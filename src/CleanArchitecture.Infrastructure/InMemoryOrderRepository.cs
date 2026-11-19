using System.Collections.Concurrent;
using CleanArchitecture.Application;
using CleanArchitecture.Domain;

namespace CleanArchitecture.Infrastructure;

// Demo-only adapter. Data is lost on restart; use a database adapter for production.
public sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly ConcurrentDictionary<Guid, Order> _orders = new();

    public Task<Order?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _orders.TryGetValue(id, out var order);
        return Task.FromResult(order);
    }

    public Task SaveAsync(Order order, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _orders[order.Id] = order;
        return Task.CompletedTask;
    }
}
