namespace Ordering.Domain.ValueObjects;
public record OrderItemId
{
    public Guid Value { get; }
    private OrderItemId(Guid value) => Value = value;
    public static OrderItemId Of(Guid value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value.ToString());
        if (value == Guid.Empty)
        {
            throw new DomainException("OrderItemId cannot be empty.");
        }

        return new OrderItemId(value);
    }
}
