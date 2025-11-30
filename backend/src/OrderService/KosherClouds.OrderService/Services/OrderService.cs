using AutoMapper;
using KosherClouds.Contracts.Orders;
using KosherClouds.OrderService.Data;
using KosherClouds.OrderService.Entities;
using KosherClouds.OrderService.DTOs.Order;
using KosherClouds.OrderService.DTOs.OrderItem;
using KosherClouds.OrderService.Parameters;
using KosherClouds.OrderService.Services.Interfaces;
using KosherClouds.OrderService.Services.External;
using KosherClouds.ServiceDefaults.Helpers;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace KosherClouds.OrderService.Services
{
    public class OrderService : IOrderService
    {
        private readonly OrderDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ISortHelperFactory _sortFactory;
        private readonly ILogger<OrderService> _logger;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ICartApiClient _cartApiClient;
        private readonly IProductApiClient _productApiClient;
        private readonly IUserApiClient _userApiClient;
        private readonly bool _isInMemory;

        public OrderService(
            OrderDbContext dbContext,
            IMapper mapper,
            ISortHelperFactory sortFactory,
            ILogger<OrderService> logger,
            IPublishEndpoint publishEndpoint,
            ICartApiClient cartApiClient,
            IProductApiClient productApiClient,
            IUserApiClient userApiClient,
            bool isInMemory = false)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _sortFactory = sortFactory;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
            _cartApiClient = cartApiClient;
            _productApiClient = productApiClient;
            _userApiClient = userApiClient;
            _isInMemory = isInMemory;
        }

        public async Task<PagedList<OrderResponseDto>> GetOrdersAsync(
            OrderParameters parameters,
            CancellationToken cancellationToken = default)
        {
            IQueryable<Order> query = _dbContext.Orders
                .Include(o => o.Items)
                .AsNoTracking();

            if (parameters.UserId.HasValue)
                query = query.Where(o => o.UserId == parameters.UserId.Value);

            if (parameters.Status.HasValue)
                query = query.Where(o => o.Status == parameters.Status.Value);

            if (parameters.PaymentType.HasValue)
                query = query.Where(o => o.PaymentType == parameters.PaymentType.Value);

            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                query = _isInMemory
                    ? query.Where(o => o.Notes != null && o.Notes.Contains(parameters.SearchTerm, StringComparison.OrdinalIgnoreCase))
                    : query.Where(o => o.Notes != null && EF.Functions.ILike(o.Notes, $"%{parameters.SearchTerm}%"));
            }

            if (parameters.MinTotalAmount.HasValue)
                query = query.Where(o => o.TotalAmount >= parameters.MinTotalAmount.Value);

            if (parameters.MaxTotalAmount.HasValue)
                query = query.Where(o => o.TotalAmount <= parameters.MaxTotalAmount.Value);

            if (parameters.MinOrderDate.HasValue)
                query = query.Where(o => o.CreatedAt >= parameters.MinOrderDate.Value);

            if (parameters.MaxOrderDate.HasValue)
                query = query.Where(o => o.CreatedAt <= parameters.MaxOrderDate.Value);

            var sortHelper = _sortFactory.Create<Order>();
            query = sortHelper.ApplySort(query, parameters.OrderBy);

            var pagedOrders = await PagedList<Order>.ToPagedListAsync(
                query,
                parameters.PageNumber,
                parameters.PageSize,
                cancellationToken);

            var orderDtos = _mapper.Map<IEnumerable<OrderResponseDto>>(pagedOrders);

            return new PagedList<OrderResponseDto>(
                orderDtos.ToList(),
                pagedOrders.TotalCount,
                pagedOrders.CurrentPage,
                pagedOrders.PageSize);
        }

        public async Task<OrderResponseDto?> GetOrderByIdAsync(
            Guid orderId,
            CancellationToken cancellationToken = default)
        {
            var order = await _dbContext.Orders
                .Include(o => o.Items)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

            return _mapper.Map<OrderResponseDto?>(order);
        }

        public async Task<OrderResponseDto> CreateOrderFromCartAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating order from cart for user {UserId}", userId);

            var cartItems = await _cartApiClient.GetCartAsync(userId, cancellationToken);

            if (cartItems == null || !cartItems.Any())
            {
                _logger.LogWarning("Cart is empty for user {UserId}", userId);
                throw new InvalidOperationException("Cart is empty");
            }

            var userInfo = await _userApiClient.GetUserAsync(userId, cancellationToken);
            if (userInfo == null)
            {
                _logger.LogWarning("Failed to fetch user info for {UserId}", userId);
                throw new InvalidOperationException("Failed to fetch user information");
            }

            if (string.IsNullOrWhiteSpace(userInfo.PhoneNumber))
            {
                _logger.LogWarning("User {UserId} has no phone number in profile", userId);
                throw new InvalidOperationException("Phone number is required. Please update your profile.");
            }

            var orderItems = new List<OrderItemCreateDto>();
            var unavailableProducts = new List<Guid>();

            foreach (var cartItem in cartItems)
            {
                var product = await _productApiClient.GetProductAsync(
                    cartItem.ProductId, cancellationToken);

                if (product == null || !product.IsAvailable)
                {
                    _logger.LogWarning(
                        "Product {ProductId} unavailable during checkout for user {UserId}",
                        cartItem.ProductId, userId);
                    unavailableProducts.Add(cartItem.ProductId);
                    continue;
                }

                orderItems.Add(new OrderItemCreateDto
                {
                    ProductId = product.Id,
                    ProductNameSnapshot = product.Name,
                    ProductNameSnapshotUk = product.NameUk,
                    UnitPriceSnapshot = product.ActualPrice,
                    Quantity = cartItem.Quantity
                });
            }

            if (!orderItems.Any())
            {
                _logger.LogWarning("No valid products in cart for user {UserId}", userId);
                throw new InvalidOperationException(
                    "No valid products found in cart. All products are unavailable.");
            }

            var orderDto = new OrderCreateDto
            {
                UserId = userId,
                ContactName = userInfo.DisplayName,
                ContactPhone = userInfo.PhoneNumber,
                ContactEmail = userInfo.Email ?? string.Empty,
                PaymentType = PaymentType.OnPickup,
                Items = orderItems
            };

            var createdOrder = await CreateDraftOrderAsync(orderDto, cancellationToken);

            _logger.LogInformation(
                "Order {OrderId} created from cart for user {UserId} with {Count} items",
                createdOrder.Id, userId, orderItems.Count);

            if (unavailableProducts.Any())
            {
                _logger.LogWarning(
                    "Order {OrderId} created but {Count} products were unavailable: {Products}",
                    createdOrder.Id, unavailableProducts.Count, string.Join(", ", unavailableProducts));
            }

            return createdOrder;
        }

        public async Task<OrderResponseDto> CreateDraftOrderAsync(
            OrderCreateDto orderDto,
            CancellationToken cancellationToken = default)
        {
            var order = _mapper.Map<Order>(orderDto);
            order.Status = OrderStatus.Draft;
            order.TotalAmount = order.Items.Sum(i => i.UnitPriceSnapshot * i.Quantity);

            await _dbContext.Orders.AddAsync(order, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Draft order {Id} created for user {UserId}", order.Id, order.UserId);

            return _mapper.Map<OrderResponseDto>(order);
        }

        public async Task<OrderResponseDto> ConfirmOrderAsync(
            Guid orderId,
            Guid userId,
            OrderConfirmDto? request,
            CancellationToken cancellationToken = default)
        {
            var order = await _dbContext.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

            if (order == null)
                throw new KeyNotFoundException($"Order with ID '{orderId}' not found.");

            if (order.UserId != userId)
                throw new UnauthorizedAccessException($"User {userId} is not authorized to confirm order {orderId}");

            if (order.Status != OrderStatus.Draft)
                throw new InvalidOperationException(
                    $"Only Draft orders can be confirmed. Current status: {order.Status}");

            order.Status = OrderStatus.Pending;

            if (request != null)
            {
                order.ContactName = request.ContactName;
                order.ContactPhone = request.ContactPhone;

                if (!string.IsNullOrEmpty(request.Notes))
                    order.Notes = request.Notes;

                order.PaymentType = request.PaymentType;
            }

            order.UpdatedAt = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

            try
            {
                await _cartApiClient.ClearCartAsync(userId, cancellationToken);
                _logger.LogInformation("Cart cleared for user {UserId} after order confirmation", userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to clear cart for user {UserId}, but order was confirmed", userId);
            }

            if (order.PaymentType == PaymentType.OnPickup)
            {
                await PublishOrderCreatedEvent(order, cancellationToken);

                _logger.LogInformation(
                    "Order {OrderId} confirmed with Payment On Pickup. OrderCreatedEvent published.",
                    orderId);
            }
            else
            {
                _logger.LogInformation(
                    "Order {OrderId} confirmed with Online Payment. Waiting for payment completion.",
                    orderId);
            }

            _logger.LogInformation(
                "Order {OrderId} confirmed by user {UserId} and moved to Pending",
                orderId, userId);

            return _mapper.Map<OrderResponseDto>(order);
        }

        public async Task MarkOrderAsPaidAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            var order = await _dbContext.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

            if (order == null)
                throw new KeyNotFoundException($"Order {orderId} not found.");

            order.Status = OrderStatus.Paid;
            order.UpdatedAt = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

            await PublishOrderCreatedEvent(order, cancellationToken);

            _logger.LogInformation("Order {OrderId} marked as Paid and OrderCreatedEvent published", orderId);
        }

        public async Task UpdateOrderAsync(
            Guid orderId,
            OrderUpdateDto orderDto,
            CancellationToken cancellationToken = default)
        {
            var order = await _dbContext.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

            if (order == null)
                throw new KeyNotFoundException($"Order with ID '{orderId}' not found.");

            if (orderDto.Status.HasValue)
                order.Status = orderDto.Status.Value;

            if (!string.IsNullOrEmpty(orderDto.Notes))
                order.Notes = orderDto.Notes;

            order.UpdatedAt = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(new OrderUpdatedEvent
            {
                OrderId = order.Id,
                Status = order.Status.ToString(),
                Notes = order.Notes,
                UpdatedAt = order.UpdatedAt
            }, cancellationToken);

            _logger.LogInformation("Order {Id} updated (Status={Status})", order.Id, order.Status);
        }

        public async Task DeleteOrderAsync(
            Guid orderId,
            CancellationToken cancellationToken = default)
        {
            var order = await _dbContext.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

            if (order == null)
                throw new KeyNotFoundException($"Order with ID '{orderId}' not found.");

            _dbContext.Orders.Remove(order);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(new OrderDeletedEvent
            {
                OrderId = orderId,
                UserId = order.UserId,
                DeletedAt = DateTimeOffset.UtcNow
            }, cancellationToken);

            _logger.LogInformation("Order {Id} deleted successfully.", orderId);
        }

        private async Task PublishOrderCreatedEvent(Order order, CancellationToken token)
        {
            await _publishEndpoint.Publish(new OrderCreatedEvent
            {
                OrderId = order.Id,
                UserId = order.UserId,
                TotalAmount = order.TotalAmount,
                CreatedAt = order.CreatedAt,
                Items = order.Items.Select(item => new OrderItemInfo
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductNameSnapshot,
                    ProductNameUk = item.ProductNameSnapshotUk,
                    UnitPrice = item.UnitPriceSnapshot,
                    Quantity = item.Quantity,
                    LineTotal = item.LineTotal
                }).ToList()
            }, token);

            _logger.LogInformation("Published OrderCreatedEvent for {Id}", order.Id);
        }
    }
}