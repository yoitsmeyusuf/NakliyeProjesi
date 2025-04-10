using Microsoft.EntityFrameworkCore;
using NakliyeApp.Data;
using NakliyeApp.Models;

namespace NakliyeApp.Services;

public class RatingService
{
    private readonly ApplicationDbContext _context;

    public RatingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HasUserRatedShipment(int userId, int shipmentId)
    {
        return await _context.Ratings.AnyAsync(r => r.UserId == userId && r.ShipmentId == shipmentId);
    }

    public async Task AddRatingAsync(Rating rating)
    {
        _context.Ratings.Add(rating);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Rating>> GetRatingsForUserAsync(int userId)
    {
        return await _context.Ratings
            .Where(r => r.UserId == userId)
            .ToListAsync();
    }
}
