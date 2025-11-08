namespace KosherClouds.ProductService.Services.Interfaces;

using KosherClouds.ProductService.DTOs.Product;
using KosherClouds.ProductService.Parameters;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

    public interface IProductService
    {
        Task<IEnumerable<ProductResponse>> GetProductsAsync(ProductParameters parameters);
        Task<ProductResponse?> GetProductByIdAsync(Guid productId);
        Task<ProductResponse> CreateProductAsync(ProductCreateRequest productRequest);
        Task UpdateProductAsync(Guid productId, ProductUpdateRequest productRequest);
        Task DeleteProductAsync(Guid productId);
        Task UpdateProductRatingAsync(Guid productId, int newRating);
    }