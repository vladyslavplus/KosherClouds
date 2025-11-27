namespace KosherClouds.ProductService.Controllers;

using KosherClouds.ProductService.DTOs.Photo;
using KosherClouds.ProductService.DTOs.Products;
using KosherClouds.ProductService.Entities.Enums;
using KosherClouds.ProductService.Parameters;
using KosherClouds.ProductService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Threading;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin, Manager")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ICloudinaryService _cloudinaryService;

    public ProductsController(
        IProductService productService,
        ICloudinaryService cloudinaryService)
    {
        _productService = productService;
        _cloudinaryService = cloudinaryService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetProducts([FromQuery] ProductParameters parameters, CancellationToken cancellationToken)
    {
        var products = await _productService.GetProductsAsync(parameters, cancellationToken);

        Response.Headers["X-Pagination"] = JsonSerializer.Serialize(new
        {
            products.TotalCount,
            products.PageSize,
            products.CurrentPage,
            products.TotalPages,
            products.HasNext,
            products.HasPrevious
        });

        return Ok(products);
    }

    [HttpGet("subcategories")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSubCategories([FromQuery] ProductCategory category, CancellationToken cancellationToken)
    {
        var result = await _productService.GetSubCategoriesAsync(category, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductById(Guid id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetProductByIdAsync(id, cancellationToken);
        if (product == null)
            return NotFound();

        return Ok(product);
    }

    [HttpGet("{id:guid}/photos")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductPhotos(Guid id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetProductByIdAsync(id, cancellationToken);
        if (product == null)
            return NotFound(new { message = $"Product with ID {id} not found" });

        return Ok(new
        {
            productId = product.Id,
            productName = product.Name,
            photos = product.Photos,
            photoCount = product.Photos.Count
        });
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] ProductCreateRequest request, CancellationToken cancellationToken)
    {
        var createdProduct = await _productService.CreateProductAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(GetProductById),
            new { id = createdProduct.Id },
            createdProduct
        );
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] ProductUpdateRequest request, CancellationToken cancellationToken)
    {
        await _productService.UpdateProductAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteProduct(Guid id, CancellationToken cancellationToken)
    {
        await _productService.DeleteProductAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/photos")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AddProductPhotos(
        Guid id,
        [FromForm] UploadMultiplePhotosRequest request,
        CancellationToken cancellationToken)
    {
        var product = await _productService.GetProductByIdAsync(id, cancellationToken);
        if (product == null)
            return NotFound(new { message = $"Product with ID {id} not found" });

        var hasFiles = request.Files != null && request.Files.Count > 0;
        var hasUrls = request.Urls != null && request.Urls.Count > 0;

        if (!hasFiles && !hasUrls)
            return BadRequest(new { message = "Either Files or Urls must be provided" });

        if (hasFiles && hasUrls)
            return BadRequest(new { message = "Provide either Files or Urls, not both" });

        List<string> newPhotoUrls;

        if (hasFiles)
        {
            newPhotoUrls = await _cloudinaryService.UploadMultiplePhotosAsync(request.Files!);
        }
        else
        {
            newPhotoUrls = await _cloudinaryService.UploadMultiplePhotosFromUrlsAsync(request.Urls!);
        }

        var updatedPhotos = product.Photos.Concat(newPhotoUrls).ToList();

        await _productService.UpdateProductAsync(id, new ProductUpdateRequest
        {
            Photos = updatedPhotos
        }, cancellationToken);

        return Ok(new
        {
            message = "Photos added successfully",
            addedPhotos = newPhotoUrls,
            totalPhotos = updatedPhotos.Count
        });
    }

    [HttpDelete("{id:guid}/photos")]
    public async Task<IActionResult> RemoveProductPhoto(
        Guid id,
        [FromQuery] string photoUrl,
        CancellationToken cancellationToken)
    {
        var product = await _productService.GetProductByIdAsync(id, cancellationToken);
        if (product == null)
            return NotFound(new { message = $"Product with ID {id} not found" });

        if (!product.Photos.Contains(photoUrl))
            return NotFound(new { message = "Photo not found in product" });

        var publicId = _cloudinaryService.ExtractPublicIdFromUrl(photoUrl);
        if (!string.IsNullOrEmpty(publicId))
        {
            await _cloudinaryService.DeletePhotoAsync(publicId);
        }

        var updatedPhotos = product.Photos.Where(p => p != photoUrl).ToList();

        await _productService.UpdateProductAsync(id, new ProductUpdateRequest
        {
            Photos = updatedPhotos
        }, cancellationToken);

        return Ok(new
        {
            message = "Photo removed successfully",
            remainingPhotos = updatedPhotos.Count
        });
    }

    [HttpPut("{id:guid}/photos")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ReplaceProductPhotos(
        Guid id,
        [FromForm] UploadMultiplePhotosRequest request,
        CancellationToken cancellationToken)
    {
        var product = await _productService.GetProductByIdAsync(id, cancellationToken);
        if (product == null)
            return NotFound(new { message = $"Product with ID {id} not found" });

        foreach (var oldPhotoUrl in product.Photos)
        {
            var publicId = _cloudinaryService.ExtractPublicIdFromUrl(oldPhotoUrl);
            if (!string.IsNullOrEmpty(publicId))
            {
                await _cloudinaryService.DeletePhotoAsync(publicId);
            }
        }

        var hasFiles = request.Files != null && request.Files.Count > 0;
        var hasUrls = request.Urls != null && request.Urls.Count > 0;

        if (!hasFiles && !hasUrls)
            return BadRequest(new { message = "Either Files or Urls must be provided" });

        if (hasFiles && hasUrls)
            return BadRequest(new { message = "Provide either Files or Urls, not both" });

        List<string> newPhotoUrls;

        if (hasFiles)
        {
            newPhotoUrls = await _cloudinaryService.UploadMultiplePhotosAsync(request.Files!);
        }
        else
        {
            newPhotoUrls = await _cloudinaryService.UploadMultiplePhotosFromUrlsAsync(request.Urls!);
        }

        await _productService.UpdateProductAsync(id, new ProductUpdateRequest
        {
            Photos = newPhotoUrls
        }, cancellationToken);

        return Ok(new
        {
            message = "Photos replaced successfully",
            newPhotos = newPhotoUrls,
            totalPhotos = newPhotoUrls.Count
        });
    }

    [HttpPost("upload-photo")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadPhoto([FromForm] UploadPhotoRequest request)
    {
        if (request.File == null && string.IsNullOrWhiteSpace(request.Url))
            return BadRequest(new { message = "Either File or Url must be provided" });

        if (request.File != null && !string.IsNullOrWhiteSpace(request.Url))
            return BadRequest(new { message = "Provide either File or Url, not both" });

        string url;

        if (request.File != null)
        {
            url = await _cloudinaryService.UploadPhotoAsync(request.File);
        }
        else
        {
            url = await _cloudinaryService.UploadPhotoFromUrlAsync(request.Url!);
        }

        return Ok(new { url });
    }

    [HttpPost("upload-photos")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadMultiplePhotos([FromForm] UploadMultiplePhotosRequest request)
    {
        var hasFiles = request.Files != null && request.Files.Count > 0;
        var hasUrls = request.Urls != null && request.Urls.Count > 0;

        if (!hasFiles && !hasUrls)
            return BadRequest(new { message = "Either Files or Urls must be provided" });

        if (hasFiles && hasUrls)
            return BadRequest(new { message = "Provide either Files or Urls, not both" });

        List<string> urls;

        if (hasFiles)
        {
            urls = await _cloudinaryService.UploadMultiplePhotosAsync(request.Files!);
        }
        else
        {
            urls = await _cloudinaryService.UploadMultiplePhotosFromUrlsAsync(request.Urls!);
        }

        return Ok(new { urls });
    }

    [HttpDelete("delete-photo")]
    public async Task<IActionResult> DeletePhoto([FromQuery] string url)
    {
        var publicId = _cloudinaryService.ExtractPublicIdFromUrl(url);

        if (string.IsNullOrEmpty(publicId))
            throw new ArgumentException("Invalid Cloudinary URL");

        var result = await _cloudinaryService.DeletePhotoAsync(publicId);

        if (!result)
            throw new KeyNotFoundException("Photo not found");

        return Ok(new { message = "Photo deleted successfully" });
    }
}