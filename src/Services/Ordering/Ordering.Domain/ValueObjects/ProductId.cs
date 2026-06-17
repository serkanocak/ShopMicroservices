namespace Ordering.Domain.ValueObjects;
public record ProductId
{
    public Guid Value { get; }
    private ProductId(Guid value) => Value = value;
    public static ProductId Of(Guid value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value.ToString());
        if (value == Guid.Empty)
        {
            throw new DomainException("ProductId cannot be empty.");
        }

        return new ProductId(value);
    }
}
