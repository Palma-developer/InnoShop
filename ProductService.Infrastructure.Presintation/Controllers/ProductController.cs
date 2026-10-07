using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Service.Abstraction;
using ProductService.Service.Abstraction.Models;
using System.Security.Claims;

namespace ProductService.Infrastructure.Presentation.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedAccessException("Не удалось определить пользователя из токена.");
            return userId;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var products = await _productService.GetAllAsync(ct);
            return Ok(products);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var product = await _productService.GetByIdAsync(id, ct);
            if (product == null) return NotFound();
            return Ok(product);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string? name,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] bool? isAvailable,
            CancellationToken ct)
        {
            var products = await _productService.SearchAsync(name, minPrice, maxPrice, isAvailable, ct);
            return Ok(products);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductRequest request, CancellationToken ct)
        {
            var userId = GetCurrentUserId();
            var product = await _productService.CreateAsync(request, userId, ct);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductRequest request, CancellationToken ct)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _productService.UpdateAsync(id, request, userId, ct);
                return NoContent();
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (UnauthorizedAccessException ex) { return Forbid(); }
        }

        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _productService.DeleteAsync(id, userId, ct);
                return NoContent();
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (UnauthorizedAccessException ex) { return Forbid(); }
        }

       
        [HttpPut("hide-by-user/{userId:int}")]
        public async Task<IActionResult> HideByUser(int userId, CancellationToken ct)
        {
            await _productService.HideProductsByUserIdAsync(userId, ct);
            return Ok($"Продукты пользователя {userId} скрыты.");
        }

        [HttpPut("show-by-user/{userId:int}")]
        public async Task<IActionResult> ShowByUser(int userId, CancellationToken ct)
        {
            await _productService.ShowProductsByUserIdAsync(userId, ct);
            return Ok($"Продукты пользователя {userId} восстановлены.");
        }
    }
}