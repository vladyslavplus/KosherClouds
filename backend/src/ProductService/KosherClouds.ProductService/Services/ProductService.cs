namespace KosherClouds.ProductService.Services;

using AutoMapper;
using KosherClouds.ProductService.Data;
using KosherClouds.ProductService.Entities;
using KosherClouds.ProductService.DTOs.Products;
using KosherClouds.ProductService.Parameters;
using KosherClouds.ProductService.Services.Interfaces;
using KosherClouds.ServiceDefaults.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class ProductService : IProductService
{
    private readonly ProductDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ISortHelperFactory _sortFactory;
    private readonly ILogger<ProductService> _logger;
    private readonly bool _isInMemory;

    public ProductService(
        ProductDbContext dbContext,
        IMapper mapper,
        ISortHelperFactory sortFactory,
        ILogger<ProductService> logger,
        bool isInMemory = false)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _sortFactory = sortFactory;
        _logger = logger;
        _isInMemory = isInMemory;
    }

    public async Task<PagedList<ProductResponse>> GetProductsAsync(ProductParameters parameters, CancellationToken cancellationToken = default)
    {
        IQueryable<Product> query = _dbContext.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(parameters.Name))
        {
            query = _isInMemory
                ? query.Where(p => p.Name.Contains(parameters.Name, StringComparison.OrdinalIgnoreCase))
                : query.Where(p => EF.Functions.ILike(p.Name, $"%{parameters.Name}%"));
        }

        if (!string.IsNullOrWhiteSpace(parameters.Category))
        {
            query = _isInMemory
                ? query.Where(p => p.Category.ToString().Equals(parameters.Category, StringComparison.OrdinalIgnoreCase))
                : query.Where(p => EF.Functions.ILike(p.Category.ToString(), $"%{parameters.Category}%"));
        }

        if (parameters.MinPrice.HasValue)
            query = query.Where(p => p.Price >= parameters.MinPrice.Value);

        if (parameters.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= parameters.MaxPrice.Value);

        if (parameters.IsAvailable.HasValue)
            query = query.Where(p => p.IsAvailable == parameters.IsAvailable.Value);

        if (parameters.IsVegetarian.HasValue)
            query = query.Where(p => p.IsVegetarian == parameters.IsVegetarian.Value);

        if (parameters.CreatedAtFrom.HasValue)
            query = query.Where(p => p.CreatedAt >= parameters.CreatedAtFrom.Value);

        if (parameters.CreatedAtTo.HasValue)
            query = query.Where(p => p.CreatedAt <= parameters.CreatedAtTo.Value);

        var sortHelper = _sortFactory.Create<Product>();
        query = sortHelper.ApplySort(query, parameters.OrderBy);

        var pagedProducts = await PagedList<Product>.ToPagedListAsync(
            query,
            parameters.PageNumber,
            parameters.PageSize,
            cancellationToken);

        var productDtos = _mapper.Map<IEnumerable<ProductResponse>>(pagedProducts);

        return new PagedList<ProductResponse>(
            productDtos.ToList(),
            pagedProducts.TotalCount,
            pagedProducts.CurrentPage,
            pagedProducts.PageSize);
    }

    public async Task<ProductResponse?> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        return _mapper.Map<ProductResponse?>(product);
    }

    public async Task<ProductResponse> CreateProductAsync(ProductCreateRequest productRequest, CancellationToken cancellationToken = default)
    {
        var product = _mapper.Map<Product>(productRequest);
        await _dbContext.Products.AddAsync(product, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product {Name} created successfully.", product.Name);

        return _mapper.Map<ProductResponse>(product);
    }

    public async Task UpdateProductAsync(Guid productId, ProductUpdateRequest productRequest, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product == null)
            throw new KeyNotFoundException($"Product with ID '{productId}' not found.");

        if (productRequest.Name != null)
            product.Name = productRequest.Name;

        if (productRequest.Description != null)
            product.Description = productRequest.Description;

        if (productRequest.Price.HasValue)
            product.Price = productRequest.Price.Value;

        if (productRequest.DiscountPrice.HasValue)
            product.DiscountPrice = productRequest.DiscountPrice.Value;

        if (productRequest.IsPromotional.HasValue)
            product.IsPromotional = productRequest.IsPromotional.Value;

        if (productRequest.Category.HasValue)
            product.Category = productRequest.Category.Value;

        if (productRequest.SubCategory != null)
            product.SubCategory = productRequest.SubCategory;

        if (productRequest.IsVegetarian.HasValue)
            product.IsVegetarian = productRequest.IsVegetarian.Value;

        if (productRequest.Ingredients != null)
            product.Ingredients = productRequest.Ingredients;

        if (productRequest.Allergens != null)
            product.Allergens = productRequest.Allergens;

        if (productRequest.Photos != null)
            product.Photos = productRequest.Photos;

        if (productRequest.IsAvailable.HasValue)
            product.IsAvailable = productRequest.IsAvailable.Value;

        if (productRequest.Rating.HasValue)
            product.Rating = productRequest.Rating.Value;

        if (productRequest.RatingCount.HasValue)
            product.RatingCount = productRequest.RatingCount.Value;

        if (productRequest.HookahDetails != null)
        {
            if (product.HookahDetails == null)
                product.HookahDetails = new HookahDetails();

            product.HookahDetails.TobaccoFlavor = productRequest.HookahDetails.TobaccoFlavor ?? product.HookahDetails.TobaccoFlavor;
            product.HookahDetails.Strength = productRequest.HookahDetails.Strength;
            product.HookahDetails.BowlType = productRequest.HookahDetails.BowlType ?? product.HookahDetails.BowlType;
            product.HookahDetails.AdditionalParams = productRequest.HookahDetails.AdditionalParams ?? product.HookahDetails.AdditionalParams;
        }

        product.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product {Id} updated successfully.", product.Id);
    }

    public async Task DeleteProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product == null)
            throw new KeyNotFoundException($"Product with ID '{productId}' not found.");

        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product {Id} deleted successfully.", productId);
    }
}