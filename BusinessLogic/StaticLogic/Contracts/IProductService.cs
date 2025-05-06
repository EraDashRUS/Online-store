using OnlineStore.BusinessLogic.StaticLogic.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineStore.BusinessLogic.StaticLogic.Contracts
{
    /// <summary>
    /// Интерфейс сервиса для работы с товарами
    /// </summary>
    public interface IProductService
    {
        
        /// <summary>
        /// Получение продукта по ID
        /// </summary>
        /// <param name="id">Идентификатор продукта</param>
        /// <returns>ProductResponseDto или null</returns>
        Task<ProductResponseDto> GetProductByIdAsync(int id);

        /// <summary>
        /// Получение списка всех продуктов
        /// </summary>
        /// <returns>Список всех продуктов</returns>
        Task<List<ProductResponseDto>> GetAllProductsAsync();

        /// <summary>
        /// Создание нового продукта
        /// </summary>
        /// <param name="productDto">DTO с данными нового продукта</param>
        /// <returns>Созданный продукт</returns>
        Task<ProductResponseDto> CreateProductAsync(ProductCreateDto productDto);

        /// <summary>
        /// Обновление существующего продукта
        /// </summary>
        /// <param name="id">Идентификатор продукта</param>
        /// <param name="productDto">DTO с обновленными данными</param>
        /// <returns>Обновленный продукт</returns>
        Task<ProductResponseDto> UpdateProductAsync(int id, ProductUpdateDto productDto);

        /// <summary>
        /// Удаление продукта
        /// </summary>
        /// <param name="id">Идентификатор продукта</param>
        /// <returns>true если удаление прошло успешно, иначе false</returns>
        Task<bool> DeleteProductAsync(int id);

        
        /// <summary>
        /// Поиск продуктов по поисковому запросу
        /// </summary>
        /// <param name="searchTerm">Поисковый запрос</param>
        /// <returns>Список найденных продуктов</returns>
        Task<List<ProductResponseDto>> SearchProductsAsync(string searchTerm);

        /// <summary>
        /// Уменьшение количества товара на складе
        /// </summary>
        /// <param name="productId">Идентификатор продукта</param>
        /// <param name="quantity">Количество для уменьшения</param>
        /// <returns>true если операция прошла успешно, иначе false</returns>
        Task<bool> ReduceStockAsync(int productId, int quantity);
    }
}
