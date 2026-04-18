using MarketplaceBackend.DTOs;

namespace MarketplaceBackend.Interfaces;

public interface IFlagService
{
    Task FlagSellerAsync(FlagSellerDto dto, int reporterId);
    Task FlagBuyerAsync(FlagBuyerDto dto, int reporterId);
}
