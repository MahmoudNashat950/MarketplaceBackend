using MarketplaceBackend.DTOs;

namespace MarketplaceBackend.Interfaces;

public interface IProductService
{
    Task<System.Collections.Generic.List<ProductDto>> GetAllAsync();
    Task<System.Collections.Generic.List<ProductDto>> SearchAsync(string query);
    Task<ProductDto?> GetByIdAsync(int id);
    Task<ProductDto> CreateAsync(CreateProductDto dto, int sellerId);
    Task<ProductDto> UpdateAsync(int id, UpdateProductDto dto, int sellerId);
    Task DeleteAsync(int id, int sellerId);
}
