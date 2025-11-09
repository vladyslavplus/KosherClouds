namespace KosherClouds.OrderService.Services;

using AutoMapper;
using KosherClouds.OrderService.Data;
using KosherClouds.OrderService.Entities;
using KosherClouds.OrderService.DTOs.Order; 
using KosherClouds.OrderService.Parameters; 
using KosherClouds.OrderService.Services.Interfaces;
using KosherClouds.ServiceDefaults.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class OrderService : IOrderService
{
    private readonly OrderDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ISortHelperFactory _sortFactory;
    private readonly ILogger<OrderService> _logger;
    private readonly bool _isInMemory;

    public OrderService(
        OrderDbContext dbContext, 
        IMapper mapper, 
        ISortHelperFactory sortFactory,
        ILogger<OrderService> logger,
        bool isInMemory = false)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _sortFactory = sortFactory; 
        _logger = logger;
        _isInMemory = isInMemory; 
    }

    
    public async Task<PagedList<OrderResponseDto>> GetOrdersAsync(
        OrderParameters parameters, 
        CancellationToken cancellationToken = default)
    {
        IQueryable<Order> query = _dbContext.Orders
            .Include(o => o.Items) 
            .Include(o => o.Payments)
            .AsNoTracking(); 

        if (!string.IsNullOrWhiteSpace(parameters.Status))
        {
            query = _isInMemory
                ? query.Where(o => o.Status.Contains(parameters.Status, StringComparison.OrdinalIgnoreCase))
                : query.Where(o => EF.Functions.ILike(o.Status, $"%{parameters.Status}%"));
        }
        
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
            .Include(o => o.Payments)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        return _mapper.Map<OrderResponseDto?>(order);
    }


    public async Task<OrderResponseDto> CreateOrderAsync(
        OrderCreateDto orderDto, 
        CancellationToken cancellationToken = default)
    {
        var order = _mapper.Map<Order>(orderDto);
        
        order.Status = "Pending"; 
        
        await _dbContext.Orders.AddAsync(order, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken); 

        _logger.LogInformation("Order {Id} created successfully.", order.Id);

        return _mapper.Map<OrderResponseDto>(order);
    }


    public async Task UpdateOrderAsync(
        Guid orderId, 
        OrderUpdateDto orderDto, 
        CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
            
        if (order == null)
            throw new KeyNotFoundException($"Order with ID '{orderId}' not found.");

        if (orderDto.Status != null)
            order.Status = orderDto.Status;
        
        if (orderDto.Notes != null)
            order.Notes = orderDto.Notes;
        if (orderDto.TotalAmount.HasValue)
            order.TotalAmount = orderDto.TotalAmount.Value;
        
        if (orderDto.PaymentMethod != null)
            order.PaymentMethod = orderDto.PaymentMethod;
        
        if (orderDto.UserId.HasValue)
            order.UserId = orderDto.UserId.Value;

        order.UpdatedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Order {Id} updated successfully.", order.Id);
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
        
        _logger.LogInformation("Order {Id} deleted successfully.", orderId);
    }
}