using Catalog.API.Exceptions;
using Catalog.API.Models;
using Catalog.API.Products.CreateProduct;
using Catalog.API.Products.DeleteProduct;
using Catalog.API.Products.GetProductById;
using Catalog.API.Products.UpdateProduct;
using FluentValidation.TestHelper;
using Marten;
using NSubstitute;
using Xunit;

namespace Catalog.API.Tests;

public class CatalogTests
{
    private readonly IDocumentSession _session = Substitute.For<IDocumentSession>();

    [Fact]
    public async Task CreateProductCommandHandler_ShouldStoreProductAndSaveChanges()
    {
        // Arrange
        _session.When(x => x.Store(Arg.Any<Product>()))
            .Do(call => {
                var firstArg = call.Args()[0];
                if (firstArg is Product p)
                {
                    if (p.Id == Guid.Empty) p.Id = Guid.NewGuid();
                }
                else if (firstArg is Product[] arr && arr.Length > 0)
                {
                    if (arr[0].Id == Guid.Empty) arr[0].Id = Guid.NewGuid();
                }
            });


        var handler = new CreateProductCommandHandler(_session);
        var command = new CreateProductCommand(
            Name: "Test Product",
            Category: new List<string> { "Electronics" },
            Description: "Test Description",
            ImageFile: "test.png",
            Price: 99.99m
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        _session.Received(1).Store(Arg.Is<Product>(p => 
            p.Name == command.Name &&
            p.Category == command.Category &&
            p.Description == command.Description &&
            p.ImageFile == command.ImageFile &&
            p.Price == command.Price
        ));
        await _session.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public void CreateProductCommandValidator_ShouldHaveErrors_WhenFieldsAreEmpty()
    {
        // Arrange
        var validator = new CreateProductCommandValidator();
        var command = new CreateProductCommand("", new List<string>(), "", "", 0);

        // Act & Assert
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Category);
        result.ShouldHaveValidationErrorFor(x => x.ImageFile);
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public async Task GetProductByIdQueryHandler_ShouldReturnProduct_WhenExists()
    {
        // Arrange
        var handler = new GetProductByIdQueryHandler(_session);
        var productId = Guid.NewGuid();
        var product = new Product { Id = productId, Name = "Test" };
        _session.LoadAsync<Product>(productId, Arg.Any<CancellationToken>()).Returns(product);

        var query = new GetProductByIdQuery(productId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result.Product);
        Assert.Equal(productId, result.Product.Id);
    }

    [Fact]
    public async Task GetProductByIdQueryHandler_ShouldThrowProductNotFoundException_WhenNotExists()
    {
        // Arrange
        var handler = new GetProductByIdQueryHandler(_session);
        var productId = Guid.NewGuid();
        _session.LoadAsync<Product>(productId, Arg.Any<CancellationToken>()).Returns((Product?)null);

        var query = new GetProductByIdQuery(productId);

        // Act & Assert
        await Assert.ThrowsAsync<ProductNotFoundException>(() => handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateProductCommandHandler_ShouldUpdateProduct_WhenExists()
    {
        // Arrange
        var handler = new UpdateProductCommandHandler(_session);
        var productId = Guid.NewGuid();
        var product = new Product { Id = productId, Name = "Old Name", Price = 10.0m };
        _session.LoadAsync<Product>(productId, Arg.Any<CancellationToken>()).Returns(product);

        var command = new UpdateProductCommand(
            Id: productId,
            Name: "New Name",
            Category: new List<string> { "Updated" },
            Description: "New Desc",
            ImageFile: "new.png",
            Price: 20.0m
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _session.Received(1).Update(Arg.Is<Product>(p => 
            p.Id == productId && 
            p.Name == "New Name" && 
            p.Price == 20.0m
        ));
        await _session.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateProductCommandHandler_ShouldThrowProductNotFoundException_WhenNotExists()
    {
        // Arrange
        var handler = new UpdateProductCommandHandler(_session);
        var productId = Guid.NewGuid();
        _session.LoadAsync<Product>(productId, Arg.Any<CancellationToken>()).Returns((Product?)null);

        var command = new UpdateProductCommand(
            Id: productId,
            Name: "New Name",
            Category: new List<string> { "Updated" },
            Description: "New Desc",
            ImageFile: "new.png",
            Price: 20.0m
        );

        // Act & Assert
        await Assert.ThrowsAsync<ProductNotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public void UpdateProductCommandValidator_ShouldHaveErrors_WhenFieldsAreInvalid()
    {
        // Arrange
        var validator = new UpdateProductCommandValidator();
        var command = new UpdateProductCommand(Guid.Empty, "", new List<string>(), "", "", 0);

        // Act & Assert
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Id);
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public async Task DeleteProductCommandHandler_ShouldDeleteProduct()
    {
        // Arrange
        var handler = new DeleteProductCommandHandler(_session);
        var productId = Guid.NewGuid();
        var command = new DeleteProductCommand(productId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _session.Received(1).Delete<Product>(productId);
        await _session.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
