namespace KosherClouds.ProductService.Services.Interfaces;

using KosherClouds.ProductService.DTOs.Product;
using KosherClouds.ProductService.DTOs.Products;
using KosherClouds.ProductService.Entities.Enums;
using KosherClouds.ProductService.Parameters;
using KosherClouds.ServiceDefaults.Helpers;
using System;
using System.Threading;
using System.Threading.Tasks;

public interface IProductService
{
    Task<PagedList<ProductResponse>> GetProductsAsync(ProductParameters parameters, CancellationToken cancellationToken = default);
    Task<List<SubCategoryDto>> GetSubCategoriesAsync(ProductCategory category, CancellationToken cancellationToken = default);
    Task<ProductResponse?> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<ProductResponse> CreateProductAsync(ProductCreateRequest productRequest, CancellationToken cancellationToken = default);
    Task UpdateProductAsync(Guid productId, ProductUpdateRequest productRequest, CancellationToken cancellationToken = default);
    Task DeleteProductAsync(Guid productId, CancellationToken cancellationToken = default);
}