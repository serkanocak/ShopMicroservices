using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Data;
using Ordering.Application.Dtos;
using Ordering.Application.Exceptions;
using Ordering.Application.Orders.Commands.CreateOrder;
using Ordering.Application.Orders.Commands.DeleteOrder;
using Ordering.Application.Orders.Commands.UpdateOrder;
using Ordering.Application.Orders.Queries.GetOrders;
using Ordering.Application.Orders.Queries.GetOrdersByName;
using Ordering.Domain.Enums;
using Ordering.Domain.Models;
using Ordering.Domain.ValueObjects;
using Ordering.Infrastructure.Data;
using Xunit;
using BuildingBlocks.Pagination;

namespace Ordering.Application.Tests;

public class ApplicationTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public ApplicationTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new ApplicationDbContext(_options);
        context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }

    private async Task<(Guid customerId, Guid productId)> SeedCustomerAndProductAsync(ApplicationDbContext context)
    {
        var customerId = Guid.NewGuid();
        var customer = Customer.Create(CustomerId.Of(customerId), "John Doe", "john.doe@example.com");
        context.Customers.Add(customer);

        var productId = Guid.NewGuid();
        var product = Product.Create(ProductId.Of(productId), "Laptop", 999.99m);
        context.Products.Add(product);

        await context.SaveChangesAsync();
        return (customerId, productId);
    }

    private OrderDto CreateTestOrderDto(Guid id, Guid customerId, Guid productId, string orderName = "ORD-0001")
    {
        var shippingAddress = new AddressDto("John", "Doe", "john@example.com", "123 Main St", "USA", "NY", "10001");
        var billingAddress = new AddressDto("John", "Doe", "john@example.com", "123 Main St", "USA", "NY", "10001");
        var payment = new PaymentDto("John Doe", "1234567890123456", "12/28", "123", 1);
        var orderItems = new List<OrderItemDto>
        {
            new OrderItemDto(Guid.Empty, productId, 2, 50.0m)
        };

        return new OrderDto(
            id,
            customerId,
            orderName,
            shippingAddress,
            billingAddress,
            payment,
            OrderStatus.Pending,
            orderItems
        );
    }

    [Fact]
    public async Task CreateOrderHandler_ShouldSaveOrderToDatabase()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var (customerId, productId) = await SeedCustomerAndProductAsync(context);
        var handler = new CreateOrderHandler(context);
        var orderDto = CreateTestOrderDto(Guid.Empty, customerId, productId);
        var command = new CreateOrderCommand(orderDto);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        var order = await context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == OrderId.Of(result.Id));
        Assert.NotNull(order);
        Assert.Equal(orderDto.OrderName, order.OrderName.Value);
        Assert.Single(order.OrderItems);
        Assert.Equal(100.0m, order.TotalPrice);
    }

    [Fact]
    public async Task UpdateOrderHandler_ShouldModifyExistingOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        Guid customerId;
        Guid productId;

        using (var context = new ApplicationDbContext(_options))
        {
            (customerId, productId) = await SeedCustomerAndProductAsync(context);

            var initialOrder = Order.Create(
                OrderId.Of(orderId),
                CustomerId.Of(customerId),
                OrderName.Of("ORD-ORIGINAL"),
                Address.Of("A", "B", "email@test.com", "Line", "Country", "State", "Zip"),
                Address.Of("A", "B", "email@test.com", "Line", "Country", "State", "Zip"),
                Payment.Of("Card", "1111222233334444", "12/28", "123", 1)
            );
            context.Orders.Add(initialOrder);
            await context.SaveChangesAsync();
        }

        using (var context = new ApplicationDbContext(_options))
        {
            var handler = new UpdateOrderHandler(context);
            var updatedOrderDto = CreateTestOrderDto(orderId, customerId, productId, "ORD-UPDATED");
            var command = new UpdateOrderCommand(updatedOrderDto);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            var updatedOrder = await context.Orders.FirstAsync();
            Assert.Equal("ORD-UPDATED", updatedOrder.OrderName.Value);
        }
    }

    [Fact]
    public async Task UpdateOrderHandler_WithNonExistingOrder_ShouldThrowOrderNotFoundException()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var (customerId, productId) = await SeedCustomerAndProductAsync(context);
        var handler = new UpdateOrderHandler(context);
        var orderDto = CreateTestOrderDto(Guid.NewGuid(), customerId, productId);
        var command = new UpdateOrderCommand(orderDto);

        // Act & Assert
        await Assert.ThrowsAsync<OrderNotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task DeleteOrderHandler_ShouldRemoveOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        using (var context = new ApplicationDbContext(_options))
        {
            var (customerId, _) = await SeedCustomerAndProductAsync(context);

            var order = Order.Create(
                OrderId.Of(orderId),
                CustomerId.Of(customerId),
                OrderName.Of("ORD-DELETE"),
                Address.Of("A", "B", "email@test.com", "Line", "Country", "State", "Zip"),
                Address.Of("A", "B", "email@test.com", "Line", "Country", "State", "Zip"),
                Payment.Of("Card", "1111222233334444", "12/28", "123", 1)
            );
            context.Orders.Add(order);
            await context.SaveChangesAsync();
        }

        using (var context = new ApplicationDbContext(_options))
        {
            var handler = new DeleteOrderHandler(context);
            var command = new DeleteOrderCommand(orderId);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            var count = await context.Orders.CountAsync();
            Assert.Equal(0, count);
        }
    }

    [Fact]
    public async Task DeleteOrderHandler_WithNonExistingOrder_ShouldThrowOrderNotFoundException()
    {
        // Arrange
        using var context = new ApplicationDbContext(_options);
        var handler = new DeleteOrderHandler(context);
        var command = new DeleteOrderCommand(Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<OrderNotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task GetOrdersHandler_ShouldReturnPaginatedOrders()
    {
        // Arrange
        using (var context = new ApplicationDbContext(_options))
        {
            var (customerId, _) = await SeedCustomerAndProductAsync(context);

            for (int i = 1; i <= 5; i++)
            {
                var order = Order.Create(
                    OrderId.Of(Guid.NewGuid()),
                    CustomerId.Of(customerId),
                    OrderName.Of($"ORD-000{i}"),
                    Address.Of("A", "B", "email@test.com", "Line", "Country", "State", "Zip"),
                    Address.Of("A", "B", "email@test.com", "Line", "Country", "State", "Zip"),
                    Payment.Of("Card", "1111222233334444", "12/28", "123", 1)
                );
                context.Orders.Add(order);
            }
            await context.SaveChangesAsync();
        }

        using (var context = new ApplicationDbContext(_options))
        {
            var handler = new GetOrdersHandler(context);
            var query = new GetOrdersQuery(new PaginationRequest(PageIndex: 1, PageSize: 2));

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result.Orders);
            Assert.Equal(5, result.Orders.Count); // Total Count
            Assert.Equal(2, result.Orders.Data.Count()); // Paginated Page Size
            Assert.Equal("ORD-0003", result.Orders.Data.First().OrderName); // Ordered and skipped
        }
    }

    [Fact]
    public async Task GetOrdersByNameHandler_ShouldReturnMatchingOrders()
    {
        // Arrange
        using (var context = new ApplicationDbContext(_options))
        {
            var (customerId, _) = await SeedCustomerAndProductAsync(context);

            context.Orders.Add(Order.Create(
                OrderId.Of(Guid.NewGuid()), CustomerId.Of(customerId), OrderName.Of("Apple Pie"),
                Address.Of("A", "B", "email@test.com", "Line", "Country", "State", "Zip"),
                Address.Of("A", "B", "email@test.com", "Line", "Country", "State", "Zip"),
                Payment.Of("Card", "1111222233334444", "12/28", "123", 1)
            ));
            context.Orders.Add(Order.Create(
                OrderId.Of(Guid.NewGuid()), CustomerId.Of(customerId), OrderName.Of("Banana Split"),
                Address.Of("A", "B", "email@test.com", "Line", "Country", "State", "Zip"),
                Address.Of("A", "B", "email@test.com", "Line", "Country", "State", "Zip"),
                Payment.Of("Card", "1111222233334444", "12/28", "123", 1)
            ));
            context.Orders.Add(Order.Create(
                OrderId.Of(Guid.NewGuid()), CustomerId.Of(customerId), OrderName.Of("Apple Crumble"),
                Address.Of("A", "B", "email@test.com", "Line", "Country", "State", "Zip"),
                Address.Of("A", "B", "email@test.com", "Line", "Country", "State", "Zip"),
                Payment.Of("Card", "1111222233334444", "12/28", "123", 1)
            ));
            await context.SaveChangesAsync();
        }

        using (var context = new ApplicationDbContext(_options))
        {
            var handler = new GetOrdersByNameHandler(context);
            var query = new GetOrdersByNameQuery("Apple");

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result.Orders);
            Assert.Equal(2, result.Orders.Count());
            Assert.All(result.Orders, order => Assert.Contains("Apple", order.OrderName));
        }
    }
}
