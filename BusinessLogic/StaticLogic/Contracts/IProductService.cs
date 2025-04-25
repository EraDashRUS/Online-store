using OnlineStore.BusinessLogic.StaticLogic.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineStore.BusinessLogic.StaticLogic.Contracts
{
    public interface IProductService
    {
        // Основные CRUD операции
        /// <summary>
        /// Получение продукта по ID
        /// </summary>
        /// <param name="id">Идентификатор продукта</param>
        /// <returns>ProductResponseDto или null</returns>
        Task<ProductResponseDto> GetProductByIdAsync(int id);
        Task<List<ProductResponseDto>> GetAllProductsAsync();
        Task<ProductResponseDto> CreateProductAsync(ProductCreateDto productDto);
        Task<ProductResponseDto> UpdateProductAsync(int id, ProductUpdateDto productDto);
        Task<bool> DeleteProductAsync(int id);

        // Специфичные методы
        Task<List<ProductResponseDto>> SearchProductsAsync(string searchTerm);
        Task<bool> ReduceStockAsync(int productId, int quantity);

    }
}
