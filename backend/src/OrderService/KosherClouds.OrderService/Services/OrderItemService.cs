namespace KosherClouds.OrderService.Services;

using KosherClouds.OrderService.Data;
using KosherClouds.OrderService.DTOs.OrderItem;
using AutoMapper;
using KosherClouds.OrderService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;


public class OrderItemService : IOrderItemService
{
    private readonly OrderDbContext _dbContext;
    private readonly IMapper _mapper;

    public OrderItemService(OrderDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }


    public async Task<IEnumerable<OrderItemResponseDto>> GetItemsByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var items = await _dbContext.OrderItems
            .Where(item => item.OrderId == orderId)
            .OrderBy(item => item.CreatedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return _mapper.Map<IEnumerable<OrderItemResponseDto>>(items);
    }

    public async Task<OrderItemResponseDto?> GetOrderItemByIdAsync(
        Guid itemId,
        CancellationToken cancellationToken = default)
    {
        var item = await _dbContext.OrderItems
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == itemId, cancellationToken);

        return _mapper.Map<OrderItemResponseDto?>(item);
    }

    public async Task UpdateOrderItemQuantityAsync(
        Guid itemId,
        int newQuantity,
        CancellationToken cancellationToken = default)
    {
        var item = await _dbContext.OrderItems
            .FirstOrDefaultAsync(i => i.Id == itemId, cancellationToken);

        if (item == null)
            throw new KeyNotFoundException($"OrderItem with ID '{itemId}' not found.");

        item.Quantity = newQuantity;
        item.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}