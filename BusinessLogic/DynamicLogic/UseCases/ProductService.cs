using OnlineStore.BusinessLogic.StaticLogic.Contracts.Exceptions;
using OnlineStore.BusinessLogic.StaticLogic.Contracts;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Storage.Data;
using OnlineStore.Storage.Models;
using OnlineStore.BusinessLogic.StaticLogic.DTOs.Product;

namespace OnlineStore.BusinessLogic.DynamicLogic.Services
{
    /// <summary>
    /// Сервис для управления товарами: фильтрация, создание, обновление, удаление, поиск и изменение остатков.
    /// </summary>
    public class ProductService(
        IRepository<Product> repository,
        ApplicationDbContext context) : IProductService
    {
        private readonly IRepository<Product> _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly ApplicationDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

        private ProductResponseDto ConvertToDto(Product product)
        {
            return new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
            };
        }

        /// <summary>
        /// Получить список товаров с учетом фильтрации, сортировки и наличия на складе.
        /// </summary>
        /// <param name="query">Параметры фильтрации и сортировки</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Список товаров</returns>
        public async Task<IEnumerable<ProductResponseDto>> GetProductsAsync(
            ProductQueryDto query,
            CancellationToken cancellationToken = default)
        {
            var queryable = _context.Products.AsQueryable();
            queryable = ApplySearchFilter(queryable, query.SearchTerm);
            queryable = ApplyPriceFilters(queryable, query.MinPrice, query.MaxPrice);
            queryable = ApplyStockFilter(queryable, query.InStock);
            queryable = ApplySorting(queryable, query.SortBy, query.SortDescending);
            return await MapToDto(queryable, cancellationToken);
        }

        private IQueryable<Product> ApplySearchFilter(IQueryable<Product> query, string? searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return query;
            return query.Where(p =>
                p.Name.Contains(searchTerm) ||
                p.Description.Contains(searchTerm));
        }

        private IQueryable<Product> ApplyPriceFilters(IQueryable<Product> query, decimal? minPrice, decimal? maxPrice)
        {
            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);
            return query;
        }

        private IQueryable<Product> ApplyStockFilter(IQueryable<Product> query, bool? inStock)
        {
            if (inStock.HasValue && inStock.Value)
                return query.Where(p => p.StockQuantity > 0);
            return query;
        }

        private static IQueryable<Product> ApplySorting(IQueryable<Product> query, string? sortBy, bool sortDescending)
        {
            return sortBy?.ToLower() switch
            {
                "name" => sortDescending
                    ? query.OrderByDescending(p => p.Name)
                    : query.OrderBy(p => p.Name),
                "price" => sortDescending
                    ? query.OrderByDescending(p => p.Price)
                    : query.OrderBy(p => p.Price),
                _ => sortDescending
                    ? query.OrderByDescending(p => p.Id)
                    : query.OrderBy(p => p.Id)
            };
        }

        private static async Task<List<ProductResponseDto>> MapToDto(IQueryable<Product> query, CancellationToken cancellationToken)
        {
            return await query
                .Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity
                })
                .ToListAsync(cancellationToken);
        }

        private static Product ConvertFromCreateDto(ProductCreateDto dto)
        {
            return new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity
            };
        }

        /// <summary>
        /// Получить товар по идентификатору.
        /// </summary>
        /// <param name="id">ID товара</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Данные о товаре</returns>
        /// <exception cref="NotFoundException">Если товар не найден</exception>
        public async Task<ProductResponseDto> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var product = await _repository.GetByIdAsync(id, cancellationToken);
            if (product == null)
                throw new NotFoundException($"Товар с ID {id} не найден");
            return ConvertToDto(product);
        }

        /// <summary>
        /// Получить все товары.
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Список товаров</returns>
        public async Task<List<ProductResponseDto>> GetAllProductsAsync(CancellationToken cancellationToken = default)
        {
            var products = await _repository.GetAllAsync(cancellationToken);
            return products.Select(ConvertToDto).ToList();
        }

        /// <summary>
        /// Создать новый товар.
        /// </summary>
        /// <param name="productDto">Данные нового товара</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Созданный товар</returns>
        public async Task<ProductResponseDto> CreateProductAsync(ProductCreateDto productDto, CancellationToken cancellationToken = default)
        {
            var product = ConvertFromCreateDto(productDto);
            await _repository.AddAsync(product, cancellationToken);
            return ConvertToDto(product);
        }

        /// <summary>
        /// Обновить существующий товар.
        /// </summary>
        /// <param name="id">ID товара</param>
        /// <param name="productDto">Данные для обновления</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Обновленный товар</returns>
        /// <exception cref="NotFoundException">Если товар не найден</exception>
        public async Task<ProductResponseDto> UpdateProductAsync(int id, ProductUpdateDto productDto, CancellationToken cancellationToken = default)
        {
            var product = await _repository.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException($"Товар с ID {id} не найден");
            UpdateProductFields(product, productDto);
            await _repository.UpdateAsync(product, cancellationToken);
            return ConvertToDto(product);
        }

        private void UpdateProductFields(Product product, ProductUpdateDto productDto)
        {
            if (productDto.Name != null)
                product.Name = productDto.Name;
            if (productDto.Price.HasValue)
                product.Price = productDto.Price.Value;
            if (productDto.StockQuantity.HasValue)
                product.StockQuantity = productDto.StockQuantity.Value;
            if (productDto.Description != null)
                product.Description = productDto.Description;
        }

        /// <summary>
        /// Удалить товар по идентификатору.
        /// </summary>
        /// <param name="id">ID товара</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>True если удалено, иначе false</returns>
        public async Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken = default)
        {
            var product = await _repository.GetByIdAsync(id, cancellationToken);
            if (product == null)
                return false;
            await _repository.DeleteAsync(id, cancellationToken);
            return true;
        }

        /// <summary>
        /// Поиск товаров по строке.
        /// </summary>
        /// <param name="searchTerm">Строка поиска</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Список найденных товаров</returns>
        public async Task<List<ProductResponseDto>> SearchProductsAsync(string searchTerm, CancellationToken cancellationToken = default)
        {
            var products = await _repository.FindAsync(
                p => p.Name.Contains(searchTerm) ||
                     (p.Description != null && p.Description.Contains(searchTerm)),
                cancellationToken
            );
            return products.Select(ConvertToDto).ToList();
        }

        /// <summary>
        /// Уменьшить остаток товара на складе.
        /// </summary>
        /// <param name="productId">ID товара</param>
        /// <param name="quantity">Количество для уменьшения</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>True если уменьшено, иначе false</returns>
        /// <exception cref="NotFoundException">Если товар не найден</exception>
        public async Task<bool> ReduceStockAsync(int productId, int quantity, CancellationToken cancellationToken = default)
        {
            var product = await _repository.GetByIdAsync(productId, cancellationToken) ?? throw new NotFoundException($"Товар с ID {productId} не найден");
            if (product.StockQuantity < quantity)
                return false;
            product.StockQuantity -= quantity;
            await _repository.UpdateAsync(product, cancellationToken);
            return true;
        }
    }
}