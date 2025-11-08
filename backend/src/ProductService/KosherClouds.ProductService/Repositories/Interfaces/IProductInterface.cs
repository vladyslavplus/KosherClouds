namespace KosherClouds.ProductService.Repositories.Interfaces;

using KosherClouds.ProductService.DTOs.Product;
using KosherClouds.ProductService.Parameters;
using KosherClouds.ProductService.Entities;

    public interface IProductRepository
    {
        Task<IEnumerable<Product>> FindAllAsync(ProductParameters parameters);
        Task<Product?> FindByIdAsync(Guid id);
        Task<Product> CreateAsync(Product entity);
        Task UpdateAsync(Product entity);
        Task DeleteAsync(Guid id);
        Task UpdateRatingAsync(Guid id, int newRating);
    }