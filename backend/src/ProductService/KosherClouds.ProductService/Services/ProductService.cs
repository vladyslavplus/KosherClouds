namespace KosherClouds.ProductService.Services;

using AutoMapper;
using KosherClouds.ProductService.Data;
using KosherClouds.ProductService.DTOs.Product;
using KosherClouds.ProductService.DTOs.Products;
using KosherClouds.ProductService.Entities;
using KosherClouds.ProductService.Entities.Enums;
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

        if (!string.IsNullOrWhiteSpace(parameters.NameUk))
        {
            query = _isInMemory
                ? query.Where(p => p.NameUk != null && p.NameUk.Contains(parameters.NameUk, StringComparison.OrdinalIgnoreCase))
                : query.Where(p => p.NameUk != null && EF.Functions.ILike(p.NameUk, $"%{parameters.NameUk}%"));
        }

        if (!string.IsNullOrWhiteSpace(parameters.Category) && Enum.TryParse<ProductCategory>(parameters.Category, true, out var categoryEnum))
        {
            query = query.Where(p => p.Category == categoryEnum);
        }

        if (!string.IsNullOrWhiteSpace(parameters.SubCategory))
        {
            query = _isInMemory
                ? query.Where(p => p.SubCategory != null && p.SubCategory.Contains(parameters.SubCategory, StringComparison.OrdinalIgnoreCase))
                : query.Where(p => p.SubCategory != null && EF.Functions.ILike(p.SubCategory, $"%{parameters.SubCategory}%"));
        }

        if (!string.IsNullOrWhiteSpace(parameters.SubCategoryUk))
        {
            query = _isInMemory
                ? query.Where(p => p.SubCategoryUk != null && p.SubCategoryUk.Contains(parameters.SubCategoryUk, StringComparison.OrdinalIgnoreCase))
                : query.Where(p => p.SubCategoryUk != null && EF.Functions.ILike(p.SubCategoryUk, $"%{parameters.SubCategoryUk}%"));
        }

        if (parameters.MinPrice.HasValue)
            query = query.Where(p => p.Price >= parameters.MinPrice.Value);

        if (parameters.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= parameters.MaxPrice.Value);

        if (parameters.IsAvailable.HasValue)
            query = query.Where(p => p.IsAvailable == parameters.IsAvailable.Value);

        if (parameters.IsVegetarian.HasValue)
            query = query.Where(p => p.IsVegetarian == parameters.IsVegetarian.Value);

        if (parameters.IsPromotional.HasValue)
            query = query.Where(p => p.IsPromotional == parameters.IsPromotional.Value);

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

    public async Task<List<SubCategoryDto>> GetSubCategoriesAsync(ProductCategory category, CancellationToken cancellationToken = default)
    {
        var subCategories = await _dbContext.Products
            .AsNoTracking()
            .Where(p => p.Category == category && p.SubCategory != null && p.SubCategory != "")
            .Select(p => new
            {
                p.SubCategory,
                p.SubCategoryUk
            })
            .Distinct()
            .ToListAsync(cancellationToken);

        return subCategories.Select(s => new SubCategoryDto
        {
            Value = s.SubCategory!,
            Label = s.SubCategory!,
            LabelUk = s.SubCategoryUk ?? s.SubCategory!
        }).ToList();
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

        if (productRequest.Name != null) product.Name = productRequest.Name;
        if (productRequest.Description != null) product.Description = productRequest.Description;
        if (productRequest.SubCategory != null) product.SubCategory = productRequest.SubCategory;
        if (productRequest.Ingredients != null) product.Ingredients = productRequest.Ingredients;
        if (productRequest.Allergens != null) product.Allergens = productRequest.Allergens;

        if (productRequest.NameUk != null) product.NameUk = productRequest.NameUk;
        if (productRequest.DescriptionUk != null) product.DescriptionUk = productRequest.DescriptionUk;
        if (productRequest.SubCategoryUk != null) product.SubCategoryUk = productRequest.SubCategoryUk;
        if (productRequest.IngredientsUk != null) product.IngredientsUk = productRequest.IngredientsUk;
        if (productRequest.AllergensUk != null) product.AllergensUk = productRequest.AllergensUk;

        if (productRequest.Price.HasValue) product.Price = productRequest.Price.Value;
        if (productRequest.DiscountPrice.HasValue) product.DiscountPrice = productRequest.DiscountPrice.Value;
        if (productRequest.IsPromotional.HasValue) product.IsPromotional = productRequest.IsPromotional.Value;
        if (productRequest.Category.HasValue) product.Category = productRequest.Category.Value;
        if (productRequest.IsVegetarian.HasValue) product.IsVegetarian = productRequest.IsVegetarian.Value;
        if (productRequest.Photos != null) product.Photos = productRequest.Photos;
        if (productRequest.IsAvailable.HasValue) product.IsAvailable = productRequest.IsAvailable.Value;
        if (productRequest.Rating.HasValue) product.Rating = productRequest.Rating.Value;
        if (productRequest.RatingCount.HasValue) product.RatingCount = productRequest.RatingCount.Value;

        if (productRequest.HookahDetails != null)
        {
            if (product.HookahDetails == null)
                product.HookahDetails = new HookahDetails();

            product.HookahDetails.TobaccoFlavor = productRequest.HookahDetails.TobaccoFlavor ?? product.HookahDetails.TobaccoFlavor;
            product.HookahDetails.TobaccoFlavorUk = productRequest.HookahDetails.TobaccoFlavorUk ?? product.HookahDetails.TobaccoFlavorUk;
            product.HookahDetails.Strength = productRequest.HookahDetails.Strength;
            product.HookahDetails.BowlType = productRequest.HookahDetails.BowlType ?? product.HookahDetails.BowlType;
            product.HookahDetails.BowlTypeUk = productRequest.HookahDetails.BowlTypeUk ?? product.HookahDetails.BowlTypeUk;
            product.HookahDetails.AdditionalParams = productRequest.HookahDetails.AdditionalParams ?? product.HookahDetails.AdditionalParams;
            product.HookahDetails.AdditionalParamsUk = productRequest.HookahDetails.AdditionalParamsUk ?? product.HookahDetails.AdditionalParamsUk;
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