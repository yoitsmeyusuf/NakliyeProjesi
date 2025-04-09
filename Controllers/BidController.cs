using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NakliyeApp.Data;
using NakliyeApp.Models;
using System.Security.Claims;

namespace NakliyeApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BidController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public BidController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/bid/by-user
    [HttpGet("by-user")]
    [Authorize]
    public async Task<IActionResult> GetUserBids()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var bids = await _context.Bids
            .Include(b => b.Shipment)
            .Where(b => b.ShipperId == userId)
            .ToListAsync();

        return Ok(bids);
    }

    // POST: api/bid
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateBid([FromBody] Bid bid)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role);

        if (role != UserType.Shipper.ToString())
            return Forbid("Sadece nakliyeciler teklif verebilir.");

        var shipment = await _context.Shipments.FindAsync(bid.ShipmentId);
        if (shipment == null)
            return NotFound("İlan bulunamadı");

        if (shipment.CustomerId == userId)
            return BadRequest("Kendi ilanınıza teklif veremezsiniz.");

        bid.ShipperId = userId;
        bid.CreatedAt = DateTime.UtcNow;

        _context.Bids.Add(bid);
        await _context.SaveChangesAsync();

        return Ok(bid);
    }

    // DELETE: api/bid/5
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteBid(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var bid = await _context.Bids.FindAsync(id);

        if (bid == null)
            return NotFound();

        if (bid.ShipperId != userId)
            return Forbid("Bu teklifi silme yetkiniz yok.");

        _context.Bids.Remove(bid);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // PUT: api/bid/5
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateBid(int id, [FromBody] Bid updatedBid)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var bid = await _context.Bids.FindAsync(id);
        if (bid == null)
            return NotFound("Teklif bulunamadı.");

        if (bid.ShipperId != userId)
            return Forbid("Bu teklifi güncelleme yetkiniz yok.");

        // Sadece fiyat güncellenebilir
        bid.Price = updatedBid.Price;
        await _context.SaveChangesAsync();

        return Ok(bid);
    }

    
}
