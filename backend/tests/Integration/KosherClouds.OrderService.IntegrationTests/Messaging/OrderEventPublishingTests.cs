using FluentAssertions;
using KosherClouds.Contracts.Orders;
using KosherClouds.OrderService.Entities;
using KosherClouds.OrderService.IntegrationTests.Infrastructure;
using KosherClouds.OrderService.Services.Interfaces;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace KosherClouds.OrderService.IntegrationTests.Messaging
{
    public class OrderEventPublishingTests : IClassFixture<OrderServiceWebApplicationFactory>, IDisposable
    {
        private readonly OrderServiceWebApplicationFactory _factory;
        private readonly IServiceScope _scope;
        private readonly IOrderService _orderService;
        private bool _disposed;

        public OrderEventPublishingTests(OrderServiceWebApplicationFactory factory)
        {
            _factory = factory;
            _scope = factory.Services.CreateScope();
            _orderService = _scope.ServiceProvider.GetRequiredService<IOrderService>();
        }

        [Fact]
        public async Task ConfirmOrder_WithOnPickupPayment_ShouldPublishOrderCreatedEvent()
        {
            var harness = _scope.ServiceProvider.GetRequiredService<ITestHarness>();
            await harness.Start();

            try
            {
                var userId = Guid.NewGuid();
                var productId = Guid.NewGuid();

                SetupMocks(userId, productId);

                var order = await _orderService.CreateOrderFromCartAsync(userId);

                MockHelper.SetupCartClearMock(_factory.CartServiceMock);

                var confirmDto = new KosherClouds.OrderService.DTOs.Order.OrderConfirmDto
                {
                    ContactName = "Test User",
                    ContactPhone = "+380123456789",
                    PaymentType = PaymentType.OnPickup
                };

                await _orderService.ConfirmOrderAsync(order.Id, userId, confirmDto);

                var published = await harness.Published.Any<OrderCreatedEvent>(
                    x => x.Context.Message.OrderId == order.Id);
                published.Should().BeTrue();

                var message = harness.Published.Select<OrderCreatedEvent>()
                    .FirstOrDefault(x => x.Context.Message.OrderId == order.Id);

                message.Should().NotBeNull();
                message!.Context.Message.OrderId.Should().Be(order.Id);
                message.Context.Message.UserId.Should().Be(userId);
                message.Context.Message.TotalAmount.Should().Be(order.TotalAmount);
            }
            finally
            {
                await harness.Stop();
            }
        }

        [Fact]
        public async Task ConfirmOrder_WithOnlinePayment_ShouldNotPublishOrderCreatedEvent()
        {
            var harness = _scope.ServiceProvider.GetRequiredService<ITestHarness>();
            await harness.Start();

            try
            {
                var userId = Guid.NewGuid();
                var productId = Guid.NewGuid();

                SetupMocks(userId, productId);

                var order = await _orderService.CreateOrderFromCartAsync(userId);

                MockHelper.SetupCartClearMock(_factory.CartServiceMock);

                var confirmDto = new KosherClouds.OrderService.DTOs.Order.OrderConfirmDto
                {
                    ContactName = "Test User",
                    ContactPhone = "+380123456789",
                    PaymentType = PaymentType.Online
                };

                await _orderService.ConfirmOrderAsync(order.Id, userId, confirmDto);

                var published = await harness.Published.Any<OrderCreatedEvent>(x => x.Context.Message.OrderId == order.Id);
                published.Should().BeFalse();
            }
            finally
            {
                await harness.Stop();
            }
        }

        [Fact]
        public async Task MarkOrderAsPaid_ShouldPublishOrderCreatedEvent()
        {
            var harness = _scope.ServiceProvider.GetRequiredService<ITestHarness>();
            await harness.Start();

            try
            {
                var userId = Guid.NewGuid();
                var productId = Guid.NewGuid();

                SetupMocks(userId, productId);

                var order = await _orderService.CreateOrderFromCartAsync(userId);

                MockHelper.SetupCartClearMock(_factory.CartServiceMock);

                var confirmDto = new KosherClouds.OrderService.DTOs.Order.OrderConfirmDto
                {
                    ContactName = "Test User",
                    ContactPhone = "+380123456789",
                    PaymentType = PaymentType.Online
                };

                var confirmedOrder = await _orderService.ConfirmOrderAsync(order.Id, userId, confirmDto);

                await _orderService.MarkOrderAsPaidAsync(confirmedOrder.Id);

                var published = await harness.Published.Any<OrderCreatedEvent>(
                    x => x.Context.Message.OrderId == confirmedOrder.Id);
                published.Should().BeTrue();

                var message = harness.Published.Select<OrderCreatedEvent>()
                    .FirstOrDefault(x => x.Context.Message.OrderId == confirmedOrder.Id);

                message.Should().NotBeNull();
                message!.Context.Message.OrderId.Should().Be(confirmedOrder.Id);
            }
            finally
            {
                await harness.Stop();
            }
        }

        private void SetupMocks(Guid userId, Guid productId)
        {
            MockHelper.SetupCartMock(_factory.CartServiceMock, userId, productId);
            MockHelper.SetupProductMock(_factory.ProductServiceMock, productId);
            MockHelper.SetupUserMock(_factory.UserServiceMock, userId);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _scope?.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}