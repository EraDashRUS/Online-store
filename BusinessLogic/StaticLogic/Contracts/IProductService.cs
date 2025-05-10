using OnlineStore.BusinessLogic.StaticLogic.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OnlineStore.BusinessLogic.StaticLogic.Contracts
{
    /// <summary>
    /// Интерфейс сервиса для работы с товарами
    /// </summary>
    public interface IProductService
    {


        Task<IEnumerable<ProductResponseDto>> GetProductsAsync(
            ProductQueryDto query,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Получение продукта по ID
        /// </summary>
        /// <param name="id">Идентификатор продукта</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>ProductResponseDto или null</returns>
        Task<ProductResponseDto> GetProductByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получение списка всех продуктов
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Список всех продуктов</returns>
        Task<List<ProductResponseDto>> GetAllProductsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Создание нового продукта
        /// </summary>
        /// <param name="productDto">DTO с данными нового продукта</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Созданный продукт</returns>
        Task<ProductResponseDto> CreateProductAsync(ProductCreateDto productDto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Обновление существующего продукта
        /// </summary>
        /// <param name="id">Идентификатор продукта</param>
        /// <param name="productDto">DTO с обновленными данными</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Обновленный продукт</returns>
        Task<ProductResponseDto> UpdateProductAsync(int id, ProductUpdateDto productDto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Удаление продукта
        /// </summary>
        /// <param name="id">Идентификатор продукта</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>true если удаление прошло успешно, иначе false</returns>
        Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken = default);

        
        /// <summary>
        /// Поиск продуктов по поисковому запросу
        /// </summary>
        /// <param name="searchTerm">Поисковый запрос</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Список найденных продуктов</returns>
        Task<List<ProductResponseDto>> SearchProductsAsync(string searchTerm, CancellationToken cancellationToken = default);

        /// <summary>
        /// Уменьшение количества товара на складе
        /// </summary>
        /// <param name="productId">Идентификатор продукта</param>
        /// <param name="quantity">Количество для уменьшения</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>true если операция прошла успешно, иначе false</returns>
        Task<bool> ReduceStockAsync(int productId, int quantity, CancellationToken cancellationToken);
    }
}
