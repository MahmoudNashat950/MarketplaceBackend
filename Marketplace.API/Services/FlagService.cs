using MarketplaceBackend.Interfaces;
using MarketplaceBackend.DTOs;
using Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceBackend.Services;

public class FlagService : IFlagService
{
    private readonly MarketplaceDbContext _db;
    public FlagService(MarketplaceDbContext db) { _db = db; }

    public async Task FlagSellerAsync(FlagSellerDto dto, int reporterId)
    {
        var flag = new Flag { ReporterId = reporterId, ReportedId = dto.sellerId, Reason = dto.reason };
        _db.Flags.Add(flag);
        await _db.SaveChangesAsync();
    }

    public async Task FlagBuyerAsync(FlagBuyerDto dto, int reporterId)
    {
        var flag = new Flag { ReporterId = reporterId, ReportedId = dto.buyerId, Reason = dto.reason };
        _db.Flags.Add(flag);
        await _db.SaveChangesAsync();
    }
}
