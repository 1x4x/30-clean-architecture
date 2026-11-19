namespace CleanArchitecture.Domain;

public sealed record OrderItem(string Sku, decimal UnitPrice, int Quantity)
{
    public decimal Subtotal => UnitPrice * Quantity;
}

public sealed class Order
{
    private readonly List<OrderItem> _items = [];
    public Guid Id { get; }
    public bool IsConfirmed { get; private set; }
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();
    public decimal Total => _items.Sum(item => item.Subtotal);

    public Order(Guid id) => Id = id;

    public void AddItem(string sku, decimal unitPrice, int quantity)
    {
        if (IsConfirmed) throw new InvalidOperationException("Confirmed orders cannot change.");
        if (string.IsNullOrWhiteSpace(sku)) throw new ArgumentException("SKU is required.", nameof(sku));
        if (unitPrice <= 0 || quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Price and quantity must be positive.");
        _items.Add(new OrderItem(sku, unitPrice, quantity));
    }

    public void Confirm()
    {
        if (IsConfirmed) throw new InvalidOperationException("Order already confirmed.");
        if (_items.Count == 0 || Total <= 0)
            throw new InvalidOperationException("Cannot confirm an empty order.");
        IsConfirmed = true;
    }
}
