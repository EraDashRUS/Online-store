using OnlineStore.BusinessLogic.StaticLogic.Contracts.Exceptions;
using OnlineStore.Models;
using OnlineStore.BusinessLogic.StaticLogic.Contracts;
using OnlineStore.BusinessLogic.StaticLogic.DTOs;

namespace OnlineStore.BusinessLogic.DynamicLogic.Services
{
    public class ProductService : IProductService
    {
        private readonly IRepository<Product> _repository;

        public ProductService(IRepository<Product> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        // Преобразование Product → ProductResponseDto
        private ProductResponseDto ConvertToDto(Product product)
        {
            return new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                // Добавьте другие свойства по необходимости
            };
        }

        // Преобразование ProductCreateDto → Product
        private Product ConvertFromCreateDto(ProductCreateDto dto)
        {
            return new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity
            };
        }

        public async Task<ProductResponseDto> GetProductByIdAsync(int id)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null)
                throw new NotFoundException($"Товар с ID {id} не найден");

            return ConvertToDto(product);
        }

        public async Task<List<ProductResponseDto>> GetAllProductsAsync()
        {
            var products = await _repository.GetAllAsync();
            return products.Select(ConvertToDto).ToList();
        }

        public async Task<ProductResponseDto> CreateProductAsync(ProductCreateDto productDto)
        {
            var product = ConvertFromCreateDto(productDto);
            await _repository.AddAsync(product);
            return ConvertToDto(product);
        }

        public async Task<ProductResponseDto> UpdateProductAsync(int id, ProductUpdateDto productDto)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null)
                throw new NotFoundException($"Товар с ID {id} не найден");

            // Частичное обновление (только указанные поля)
            if (productDto.Name != null)
                product.Name = productDto.Name;

            if (productDto.Price.HasValue)
                product.Price = productDto.Price.Value;

            if (productDto.StockQuantity.HasValue)
                product.StockQuantity = productDto.StockQuantity.Value;

            if (productDto.Description != null)
                product.Description = productDto.Description;

            await _repository.UpdateAsync(product);
            return ConvertToDto(product);
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null)
                return false;

            await _repository.DeleteAsync(id);
            return true;
        }

        public async Task<List<ProductResponseDto>> SearchProductsAsync(string searchTerm)
        {
            var products = await _repository.FindAsync(p =>
                p.Name.Contains(searchTerm) ||
                (p.Description != null && p.Description.Contains(searchTerm))
            );

            return products.Select(ConvertToDto).ToList();
        }

        public async Task<bool> ReduceStockAsync(int productId, int quantity)
        {
            var product = await _repository.GetByIdAsync(productId);
            if (product == null)
                throw new NotFoundException($"Товар с ID {productId} не найден");

            if (product.StockQuantity < quantity)
                return false;

            product.StockQuantity -= quantity;
            await _repository.UpdateAsync(product);
            return true;
        }
    }
}