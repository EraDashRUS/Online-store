using Microsoft.AspNetCore.Mvc;
using OnlineStore.BusinessLogic.StaticLogic.Contracts;
using OnlineStore.BusinessLogic.StaticLogic.Contracts.Exceptions;
using Microsoft.AspNetCore.Authorization;
using OnlineStore.BusinessLogic.StaticLogic.DTOs.Product;

namespace OnlineStore.Api.Controllers
{
    /// <summary>
    /// API для управления товарами: поиск, просмотр, создание, обновление и удаление.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        /// <summary>
        /// Конструктор контроллера товаров.
        /// </summary>
        /// <param name="productService">Сервис работы с товарами.</param>
        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Поиск товаров по фильтру.
        /// </summary>
        /// <param name="query">Параметры поиска.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Список найденных товаров.</returns>
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductResponseDto>>> SearchProducts(
            [FromQuery] ProductQueryDto query,
            CancellationToken cancellationToken)
        {
            var products = await _productService.GetProductsAsync(query, cancellationToken);
            return Ok(products);
        }

        /// <summary>
        /// Получить все товары.
        /// </summary>
        /// <returns>Список всех товаров.</returns>
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        /// <summary>
        /// Получить товар по идентификатору.
        /// </summary>
        /// <param name="id">ID товара.</param>
        /// <returns>Информация о товаре или 404.</returns>
        [Authorize(Policy = "AdminOnly")]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductResponseDto>> GetProduct(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                return Ok(product);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Обновить данные товара.
        /// </summary>
        /// <param name="id">ID товара.</param>
        /// <param name="productDto">Данные для обновления.</param>
        /// <returns>204, 400 или 404.</returns>
        [Authorize(Policy = "AdminOnly")]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutProduct(int id, ProductUpdateDto productDto)
        {
            if (!IsProductIdValid(id, productDto.Id))
                return BadRequest("ID в пути не совпадает с ID товара");

            return await TryUpdateProduct(id, productDto);
        }

        private static bool IsProductIdValid(int routeId, int dtoId)
        {
            return routeId == dtoId;
        }

        private async Task<IActionResult> TryUpdateProduct(int id, ProductUpdateDto productDto)
        {
            try
            {
                await _productService.UpdateProductAsync(id, productDto);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Создать новый товар.
        /// </summary>
        /// <param name="productDto">Данные нового товара.</param>
        /// <returns>Созданный товар.</returns>
        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<ProductResponseDto>> PostProduct(ProductCreateDto productDto)
        {
            var createdProduct = await _productService.CreateProductAsync(productDto);
            return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, createdProduct);
        }

        /// <summary>
        /// Удалить товар по идентификатору.
        /// </summary>
        /// <param name="id">ID товара.</param>
        /// <returns>204 или 404.</returns>
        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            return await TryDeleteProduct(id);
        }

        private async Task<IActionResult> TryDeleteProduct(int id)
        {
            try
            {
                await _productService.DeleteProductAsync(id);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}