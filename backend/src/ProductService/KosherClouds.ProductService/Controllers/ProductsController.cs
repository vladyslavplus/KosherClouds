namespace KosherClouds.ProductService.Controllers;

using Microsoft.AspNetCore.Mvc;
using KosherClouds.ProductService.Services.Interfaces;
using KosherClouds.ProductService.DTOs.Products;
using KosherClouds.ProductService.Parameters;
using System.Text.Json;
using System.Threading;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin, Manager")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
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

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductById(Guid id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetProductByIdAsync(id, cancellationToken);
        if (product == null)
            return NotFound();

        return Ok(product);
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
}