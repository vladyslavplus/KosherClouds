namespace KosherClouds.ProductService.Repositories;

using Microsoft.EntityFrameworkCore;
using KosherClouds.ProductService.Repositories.Interfaces;
using KosherClouds.ProductService.Parameters;
using KosherClouds.ProductService.Data;
using KosherClouds.ProductService.Entities;


    public class ProductRepository : IProductRepository
    {
        private readonly ProductDbContext _context;

        public ProductRepository(ProductDbContext context)
        {
            _context = context;
        }
        public async Task<Product> CreateAsync(Product entity)
        {
            _context.Products.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(Product entity)
        {
            _context.Products.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _context.Products.FindAsync(id);
            if (entity != null)
            {
                _context.Products.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
        
        public async Task<Product?> FindByIdAsync(Guid id)
        {
            return await _context.Products
                .Include(p => p.HookahDetails)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Product>> FindAllAsync(ProductParameters parameters)
        {
            var products = _context.Products
                .Include(p => p.HookahDetails)
                .AsQueryable();


            if (parameters.IsAvailable)
            {
                products = products.Where(p => p.IsAvailable);
            }

            if (parameters.IsVegetarian)
            {
                products = products.Where(p => p.SubCategory == "Vegetarian");
            }
            
            if (parameters.IsHookah)
            {
                products = products.Where(p => p.Category == Entities.Enums.ProductCategory.Hookah);
            }

            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                var search = parameters.SearchTerm.Trim().ToLower();
                products = products.Where(p => p.Name.ToLower().Contains(search) || 
                                               p.Description.ToLower().Contains(search));
            }

            return await products
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();
        }

        public async Task UpdateRatingAsync(Guid id, int newRating)
        {
            var product = await _context.Products.FindAsync(id);
            
            if (product != null)
            {
                var currentRatingSum = product.Rating * product.RatingCount;
                product.RatingCount += 1;
                
                product.Rating = (currentRatingSum + newRating) / product.RatingCount;
                
                await _context.SaveChangesAsync();
            }
        }
    }