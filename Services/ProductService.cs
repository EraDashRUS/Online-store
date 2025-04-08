using OnlineStore.Contracts;
using OnlineStore.Contracts.Exceptions;
using OnlineStore.DTOs;
using OnlineStore.Models;
using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineStore.Services
{
    public class ProductService : IProductService
    {
        private readonly IRepository<Product> _repository;
        private readonly IMapper _mapper;

        public ProductService(IRepository<Product> repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ProductResponseDto> GetProductByIdAsync(int id)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null)
                throw new NotFoundException($"Товар с ID {id} не найден");

            return _mapper.Map<ProductResponseDto>(product);
        }

        public async Task<List<ProductResponseDto>> GetAllProductsAsync()
        {
            var products = await _repository.GetAllAsync();
            return _mapper.Map<List<ProductResponseDto>>(products);
        }

        public async Task<ProductResponseDto> CreateProductAsync(ProductCreateDto productDto)
        {
            var product = _mapper.Map<Product>(productDto);
            await _repository.AddAsync(product);
            return _mapper.Map<ProductResponseDto>(product);
        }

        public async Task<ProductResponseDto> UpdateProductAsync(int id, ProductUpdateDto productDto)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null)
                throw new NotFoundException($"Товар с ID {id} не найден");

            _mapper.Map(productDto, product);
            await _repository.UpdateAsync(product);
            return _mapper.Map<ProductResponseDto>(product);
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

            return _mapper.Map<List<ProductResponseDto>>(products);
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