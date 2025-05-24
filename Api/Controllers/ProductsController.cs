using Microsoft.AspNetCore.Mvc;
using OnlineStore.BusinessLogic.StaticLogic.Contracts;
using OnlineStore.BusinessLogic.StaticLogic.Contracts.Exceptions;
using OnlineStore.BusinessLogic.StaticLogic.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace OnlineStore.Api.Controllers
{
    /// <summary>
    /// Контроллер для управления товарами
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        /// <summary>
        /// Инициализирует новый экземпляр контроллера товаров
        /// </summary>
        /// <param name="productService">Сервис для работы с товарами</param>
        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

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
        /// Получает список всех товаров
        /// </summary>
        /// <returns>Список товаров</returns>
        /// <response code="200">Успешно возвращен список товаров</response>
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        /// <summary>
        /// Получает товар по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор товара</param>
        /// <returns>Данные товара</returns>
        /// <response code="200">Товар найден</response>
        /// <response code="404">Товар не найден</response>
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
        /// Обновляет данные товара
        /// </summary>
        /// <param name="id">Идентификатор товара</param>
        /// <param name="productDto">Обновленные данные товара</param>
        /// <returns>Результат операции</returns>
        /// <response code="204">Товар успешно обновлен</response>
        /// <response code="400">Неверный идентификатор</response>
        /// <response code="404">Товар не найден</response>
        [Authorize(Policy = "AdminOnly")]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutProduct(int id, ProductUpdateDto productDto)
        {
            if (id != productDto.Id)
            {
                return BadRequest("ID в пути не совпадает с ID товара");
            }

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
        /// Создает новый товар
        /// </summary>
        /// <param name="productDto">Данные нового товара</param>
        /// <returns>Созданный товар</returns>
        /// <response code="201">Товар успешно создан</response>
        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<ProductResponseDto>> PostProduct(ProductCreateDto productDto)
        {
            var createdProduct = await _productService.CreateProductAsync(productDto);
            return CreatedAtAction("GetProduct", new { id = createdProduct.Id }, createdProduct);
        }

        /// <summary>
        /// Удаляет товар
        /// </summary>
        /// <param name="id">Идентификатор товара</param>
        /// <returns>Результат операции</returns>
        /// <response code="204">Товар успешно удален</response>
        /// <response code="404">Товар не найден</response>
        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProduct(int id)
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