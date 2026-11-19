using CleanArchitecture.Domain;

namespace CleanArchitecture.Application;

public interface IOrderRepository
{
    Task<Order?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveAsync(Order order, CancellationToken cancellationToken = default);
}

public sealed class CreateOrder(IOrderRepository repository)
{
    public async Task<Guid> ExecuteAsync(string sku, decimal unitPrice, int quantity, CancellationToken ct = default)
    {
        var order = new Order(Guid.NewGuid());
        order.AddItem(sku, unitPrice, quantity);
        await repository.SaveAsync(order, ct);
        return order.Id;
    }
}

public sealed class ConfirmOrder(IOrderRepository repository)
{
    public async Task ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var order = await repository.GetAsync(id, ct)
            ?? throw new KeyNotFoundException("Order not found.");
        order.Confirm();
        await repository.SaveAsync(order, ct);
    }
}
