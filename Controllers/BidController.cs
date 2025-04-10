using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NakliyeApp.Data;
using NakliyeApp.DTOs;
using NakliyeApp.Models;
using System.Security.Claims;

namespace NakliyeApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BidController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<BidController> _logger;

    public BidController(ApplicationDbContext context, ILogger<BidController> logger)
    {
        _context = context;
        _logger = logger;
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
    public async Task<IActionResult> CreateBid([FromBody] BidDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (role != UserType.Shipper.ToString())
                return Forbid("Sadece nakliyeciler teklif verebilir.");

            var shipment = await _context.Shipments.FindAsync(dto.ShipmentId);
            if (shipment == null)
                return NotFound("İlan bulunamadı");

            if (shipment.CustomerId == userId)
                return BadRequest("Kendi ilanınıza teklif veremezsiniz.");

            if (shipment.Status != ShipmentStatus.Active)
                return BadRequest("Bu ilana artık teklif verilemez.");

            var existingBid = await _context.Bids
                .FirstOrDefaultAsync(b => b.ShipmentId == dto.ShipmentId && b.ShipperId == userId);

            if (existingBid != null)
                return BadRequest("Aynı ilana birden fazla teklif veremezsiniz.");

            var bid = new Bid
            {
                Price = dto.Price,
                ShipmentId = dto.ShipmentId,
                ShipperId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Bids.Add(bid);

            // Create a notification for the shipment owner
            var notification = new Notification
            {
                UserId = shipment.CustomerId,
                CreatedByUserId = userId,
                Message = $"Yeni bir teklif aldınız: {bid.Price} TL",
            };
            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Yeni teklif oluşturuldu: {BidId}", bid.Id);

            return Ok(bid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Teklif oluşturulurken bir hata oluştu.");
            return StatusCode(500, "Bir hata oluştu. Lütfen tekrar deneyin.");
        }
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

        // Notify the shipment owner about the updated bid
        var shipment = await _context.Shipments.FindAsync(bid.ShipmentId);
        if (shipment != null)
        {
            var notification = new Notification
            {
                UserId = shipment.CustomerId,
                CreatedByUserId = userId,
                Message = $"Teklif güncellendi: {bid.Price} TL",
            };
            _context.Notifications.Add(notification);
        }

        await _context.SaveChangesAsync();

        return Ok(bid);
    }
}
