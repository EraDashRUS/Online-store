using OnlineStore.BusinessLogic.StaticLogic.DTOs.Product;

namespace OnlineStore.BusinessLogic.StaticLogic.Contracts
{

    public interface IProductService
    {
        Task<IEnumerable<ProductResponseDto>> GetProductsAsync(
            ProductQueryDto query,
            CancellationToken cancellationToken = default);

        Task<ProductResponseDto> GetProductByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<List<ProductResponseDto>> GetAllProductsAsync(CancellationToken cancellationToken = default);

        Task<ProductResponseDto> CreateProductAsync(ProductCreateDto productDto, CancellationToken cancellationToken = default);

        Task<ProductResponseDto> UpdateProductAsync(int id, ProductUpdateDto productDto, CancellationToken cancellationToken = default);

        Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken = default);

        Task<List<ProductResponseDto>> SearchProductsAsync(string searchTerm, CancellationToken cancellationToken = default);

        Task<bool> ReduceStockAsync(int productId, int quantity, CancellationToken cancellationToken);
    }
}
