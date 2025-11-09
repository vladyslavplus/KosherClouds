namespace KosherClouds.ProductService.Services;

using KosherClouds.ProductService.DTOs.Product;
using KosherClouds.ProductService.Services.Interfaces;
using KosherClouds.ProductService.Parameters;
using KosherClouds.ProductService.Entities;
using KosherClouds.ProductService.Repositories.Interfaces;
using AutoMapper;

    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        private readonly IMapper _mapper;

        public ProductService(IProductRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductResponse>> GetProductsAsync(ProductParameters parameters)
        {
            var productEntities = await _repository.FindAllAsync(parameters);
            
            return _mapper.Map<IEnumerable<ProductResponse>>(productEntities);
        }

        public async Task<ProductResponse?> GetProductByIdAsync(Guid productId)
        {
            var productEntity = await _repository.FindByIdAsync(productId);

            if (productEntity == null)
            {
                return null;
            }

            return _mapper.Map<ProductResponse>(productEntity);
        }

        public async Task<ProductResponse> CreateProductAsync(ProductCreateRequest productRequest)
        {
            var productEntity = _mapper.Map<Product>(productRequest);
            
            productEntity.Id = Guid.NewGuid();
            productEntity.Rating = 0.0;
            productEntity.RatingCount = 0;
            
            var createdEntity = await _repository.CreateAsync(productEntity);
            
            return _mapper.Map<ProductResponse>(createdEntity);
        }

        public async Task UpdateProductAsync(Guid productId, ProductUpdateRequest productRequest)
        {
            var existingEntity = await _repository.FindByIdAsync(productId);

            if (existingEntity == null)
            {
                throw new KeyNotFoundException($"Product with ID {productId} not found.");
            }
            
            _mapper.Map(productRequest, existingEntity);
            
            await _repository.UpdateAsync(existingEntity);
        }

        public async Task DeleteProductAsync(Guid productId)
        {
            var existingEntity = await _repository.FindByIdAsync(productId);
            
            if (existingEntity == null)
            {
                throw new KeyNotFoundException($"Product with ID {productId} not found.");
            }
            
            await _repository.DeleteAsync(productId);
        }

        public async Task UpdateProductRatingAsync(Guid productId, int newRating)
        {
            await _repository.UpdateRatingAsync(productId, newRating);
        }
    }