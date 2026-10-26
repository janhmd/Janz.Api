using Service.Helpers.DTOs.Products;
using Microsoft.AspNetCore.Mvc;
using Service.Services.Interfaces;

namespace Janz.Api.Controllers.Client
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // Filtrsiz çağırılanda bütün görünən məhsullar qayıdır (əvvəlki kimi)
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ProductFilterDto filter)
        {
            return Ok(await _productService.SearchAsync(filter));
        }

        [HttpGet("category/{slug}")]
        public async Task<IActionResult> GetByCategory(string slug)
        {
            return Ok(await _productService.GetByCategorySlugAsync(slug));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            return Ok(await _productService.GetByIdAsync(id));
        }
    }
}
