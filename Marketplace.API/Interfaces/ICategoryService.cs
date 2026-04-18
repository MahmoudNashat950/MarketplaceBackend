using MarketplaceBackend.DTOs;

namespace MarketplaceBackend.Interfaces;

public interface ICategoryService
{
    Task<System.Collections.Generic.List<CategoryDto>> GetAllAsync();
    Task<CategoryDto> CreateAsync(CreateCategoryDto dto);
}
