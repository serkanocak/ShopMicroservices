using Ordering.Domain.Models;
using Ordering.Domain.ValueObjects;
using Ordering.Domain.Events;
using Ordering.Domain.Enums;
using Xunit;

namespace Ordering.Domain.Tests;

public class CustomerTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldReturnCustomer()
    {
        // Arrange
        var customerId = CustomerId.Of(Guid.NewGuid());
        var name = "John Doe";
        var email = "john.doe@example.com";

        // Act
        var customer = Customer.Create(customerId, name, email);

        // Assert
        Assert.NotNull(customer);
        Assert.Equal(customerId, customer.Id);
        Assert.Equal(name, customer.Name);
        Assert.Equal(email, customer.Email);
    }

    [Theory]
    [InlineData("", "test@test.com")]
    [InlineData("   ", "test@test.com")]
    [InlineData("John", "")]
    [InlineData("John", "   ")]
    public void Create_WithInvalidParameters_ShouldThrowArgumentException(string name, string email)
    {
        // Arrange
        var customerId = CustomerId.Of(Guid.NewGuid());

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Customer.Create(customerId, name, email));
    }
}

public class ProductTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldReturnProduct()
    {
        // Arrange
        var productId = ProductId.Of(Guid.NewGuid());
        var name = "Laptop";
        var price = 999.99m;

        // Act
        var product = Product.Create(productId, name, price);

        // Assert
        Assert.NotNull(product);
        Assert.Equal(productId, product.Id);
        Assert.Equal(name, product.Name);
        Assert.Equal(price, product.Price);
    }

    [Theory]
    [InlineData("", 100)]
    [InlineData("   ", 100)]
    public void Create_WithInvalidName_ShouldThrowArgumentException(string name, decimal price)
    {
        // Arrange
        var productId = ProductId.Of(Guid.NewGuid());

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Product.Create(productId, name, price));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10.5)]
    public void Create_WithInvalidPrice_ShouldThrowArgumentOutOfRangeException(decimal price)
    {
        // Arrange
        var productId = ProductId.Of(Guid.NewGuid());
        var name = "Laptop";

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => Product.Create(productId, name, price));
    }
}

public class OrderTests
{
    private readonly CustomerId _customerId = CustomerId.Of(Guid.NewGuid());
    private readonly OrderName _orderName = OrderName.Of("ORD-12345");
    private readonly Address _address = Address.Of("John", "Doe", "john@example.com", "123 Main St", "USA", "NY", "10001");
    private readonly Payment _payment = Payment.Of("John Doe", "1234567890123456", "12/28", "123", 1);

    [Fact]
    public void Create_ShouldReturnOrderWithOrderCreatedEvent()
    {
        // Arrange
        var orderId = OrderId.Of(Guid.NewGuid());

        // Act
        var order = Order.Create(orderId, _customerId, _orderName, _address, _address, _payment);

        // Assert
        Assert.NotNull(order);
        Assert.Equal(orderId, order.Id);
        Assert.Equal(_customerId, order.CustomerId);
        Assert.Equal(_orderName, order.OrderName);
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Single(order.DomainEvents);
        Assert.IsType<OrderCreatedEvent>(order.DomainEvents.First());
    }

    [Fact]
    public void Update_ShouldModifyOrderPropertiesAndAddOrderUpdatedEvent()
    {
        // Arrange
        var orderId = OrderId.Of(Guid.NewGuid());
        var order = Order.Create(orderId, _customerId, _orderName, _address, _address, _payment);
        order.ClearDomainEvents();

        var newOrderName = OrderName.Of("ORD-54321");
        var newAddress = Address.Of("Jane", "Doe", "jane@example.com", "456 Side St", "USA", "CA", "90210");
        var newPayment = Payment.Of("Jane Doe", "6543210987654321", "11/29", "321", 2);
        var newStatus = OrderStatus.Completed;

        // Act
        order.Update(newOrderName, newAddress, newAddress, newPayment, newStatus);

        // Assert
        Assert.Equal(newOrderName, order.OrderName);
        Assert.Equal(newAddress, order.ShippingAddress);
        Assert.Equal(newAddress, order.BillingAddress);
        Assert.Equal(newPayment, order.Payment);
        Assert.Equal(newStatus, order.Status);
        Assert.Single(order.DomainEvents);
        Assert.IsType<OrderUpdatedEvent>(order.DomainEvents.First());
    }

    [Fact]
    public void Add_ShouldAddOrderItemAndRecalculateTotalPrice()
    {
        // Arrange
        var orderId = OrderId.Of(Guid.NewGuid());
        var order = Order.Create(orderId, _customerId, _orderName, _address, _address, _payment);
        var productId1 = ProductId.Of(Guid.NewGuid());
        var productId2 = ProductId.Of(Guid.NewGuid());

        // Act
        order.Add(productId1, 2, 50.0m);
        order.Add(productId2, 1, 100.0m);

        // Assert
        Assert.Equal(2, order.OrderItems.Count);
        Assert.Equal(200.0m, order.TotalPrice);
        Assert.Contains(order.OrderItems, item => item.ProductId == productId1 && item.Quantity == 2 && item.Price == 50.0m);
        Assert.Contains(order.OrderItems, item => item.ProductId == productId2 && item.Quantity == 1 && item.Price == 100.0m);
    }

    [Theory]
    [InlineData(0, 50.0)]
    [InlineData(-1, 50.0)]
    [InlineData(2, 0.0)]
    [InlineData(2, -10.0)]
    public void Add_WithInvalidQuantityOrPrice_ShouldThrowArgumentOutOfRangeException(int quantity, decimal price)
    {
        // Arrange
        var orderId = OrderId.Of(Guid.NewGuid());
        var order = Order.Create(orderId, _customerId, _orderName, _address, _address, _payment);
        var productId = ProductId.Of(Guid.NewGuid());

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => order.Add(productId, quantity, price));
    }

    [Fact]
    public void Remove_ShouldRemoveOrderItemAndRecalculateTotalPrice()
    {
        // Arrange
        var orderId = OrderId.Of(Guid.NewGuid());
        var order = Order.Create(orderId, _customerId, _orderName, _address, _address, _payment);
        var productId1 = ProductId.Of(Guid.NewGuid());
        var productId2 = ProductId.Of(Guid.NewGuid());
        order.Add(productId1, 2, 50.0m);
        order.Add(productId2, 1, 100.0m);

        // Act
        order.Remove(productId1);

        // Assert
        Assert.Single(order.OrderItems);
        Assert.Equal(100.0m, order.TotalPrice);
        Assert.DoesNotContain(order.OrderItems, item => item.ProductId == productId1);
        Assert.Contains(order.OrderItems, item => item.ProductId == productId2);
    }

    [Fact]
    public void Remove_WithNonExistingProduct_ShouldDoNothing()
    {
        // Arrange
        var orderId = OrderId.Of(Guid.NewGuid());
        var order = Order.Create(orderId, _customerId, _orderName, _address, _address, _payment);
        var productId1 = ProductId.Of(Guid.NewGuid());
        var productId2 = ProductId.Of(Guid.NewGuid());
        order.Add(productId1, 2, 50.0m);

        // Act
        order.Remove(productId2);

        // Assert
        Assert.Single(order.OrderItems);
        Assert.Equal(100.0m, order.TotalPrice);
    }
}
