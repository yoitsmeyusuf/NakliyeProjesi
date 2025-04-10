using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NakliyeApp.Data;
using NakliyeApp.Models;
using System.Security.Claims;

namespace NakliyeApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShipmentController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ShipmentController> _logger;

    public ShipmentController(ApplicationDbContext context, ILogger<ShipmentController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/shipments
    [HttpGet]
    public async Task<IActionResult> GetShipments()
    {
        var shipments = await _context.Shipments
            .Include(s => s.Bids)
            .Include(s => s.Customer)
            .ToListAsync();

        return Ok(shipments);
    }

    // GET: api/shipments/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetShipment(int id)
    {
        var shipment = await _context.Shipments
            .Include(s => s.Bids)
            .Include(s => s.Customer)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (shipment == null)
            return NotFound();

        return Ok(shipment);
    }

    // POST: api/shipments
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateShipment([FromBody] Shipment shipment)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var userType = User.FindFirstValue(ClaimTypes.Role);

            if (userType != UserType.Customer.ToString())
                return Forbid("Sadece ilan veren kullanıcılar ilan oluşturabilir.");

            shipment.CustomerId = userId;
            shipment.PickupDate = shipment.PickupDate.ToUniversalTime();
            shipment.Status = ShipmentStatus.Active; // Ensure new shipments are active

            _context.Shipments.Add(shipment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Yeni ilan oluşturuldu: {ShipmentId}", shipment.Id);

            return CreatedAtAction(nameof(GetShipment), new { id = shipment.Id }, shipment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "İlan oluşturulurken bir hata oluştu.");
            return StatusCode(500, "Bir hata oluştu. Lütfen tekrar deneyin.");
        }
    }

    // GET: api/shipments/5/bids
    [HttpGet("{id}/bids")]
    public async Task<IActionResult> GetBidsForShipment(int id)
    {
        var shipment = await _context.Shipments
            .Include(s => s.Bids)
            .ThenInclude(b => b.Shipper)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (shipment == null)
            return NotFound("İlan bulunamadı");

        return Ok(shipment.Bids);
    }

    [Authorize(Roles = "Customer")]
    [HttpPost("{shipmentId}/accept-bid/{bidId}")]
    public async Task<IActionResult> AcceptBidA(int shipmentId, int bidId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var shipment = await _context.Shipments
            .Include(s => s.Bids)
            .FirstOrDefaultAsync(s => s.Id == shipmentId && s.CustomerId == userId);

        if (shipment == null)
            return NotFound("İlan bulunamadı.");

        var bid = shipment.Bids.FirstOrDefault(b => b.Id == bidId);
        if (bid == null)
            return NotFound("Teklif bu ilana ait değil.");

        shipment.AcceptedBidId = bidId;
        shipment.Status = ShipmentStatus.InTransit; // Mark shipment as "In Transit"
        await _context.SaveChangesAsync();

        return Ok("Teklif kabul edildi ve ilan 'Yolda' olarak işaretlendi.");
    }

    [Authorize(Roles = "Customer")]
    [HttpPost("{shipmentId}/complete")]
    public async Task<IActionResult> CompleteShipment(int shipmentId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var shipment = await _context.Shipments
            .FirstOrDefaultAsync(s => s.Id == shipmentId && s.CustomerId == userId);

        if (shipment == null)
            return NotFound("İlan bulunamadı.");

        if (shipment.AcceptedBidId == null)
            return BadRequest("Henüz kabul edilmiş bir teklif yok.");

        shipment.IsCompleted = true;
        shipment.Status = ShipmentStatus.Completed; // Mark shipment as completed

        // Create a notification for the shipper
        if (shipment.AcceptedBid != null)
        {
            var notification = new Notification
            {
                UserId = shipment.AcceptedBid.ShipperId,
                Message = "İlan başarıyla tamamlandı.",
            };
            _context.Notifications.Add(notification);
        }

        await _context.SaveChangesAsync();

        return Ok("İş başarıyla tamamlandı.");
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchShipments([FromQuery] string? title, [FromQuery] string? location, [FromQuery] ShipmentStatus? status)
    {
        var query = _context.Shipments.AsQueryable();

        if (!string.IsNullOrEmpty(title))
            query = query.Where(s => s.Title.Contains(title));

        if (!string.IsNullOrEmpty(location))
            query = query.Where(s => s.PickupLocation.Contains(location) || s.DeliveryLocation.Contains(location));

        if (status.HasValue)
            query = query.Where(s => s.Status == status);

        var shipments = await query
            .Include(s => s.Customer)
            .Include(s => s.Bids)
            .ToListAsync();

        return Ok(shipments);
    }

    [HttpPost("{id}/upload-photo")]
    [Authorize]
    public async Task<IActionResult> UploadShipmentPhoto(int id, IFormFile photo)
    {
        var shipment = await _context.Shipments.FindAsync(id);
        if (shipment == null)
            return NotFound("İlan bulunamadı.");

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "shipment-photos");
        Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid()}_{photo.FileName}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await photo.CopyToAsync(stream);
        }

        shipment.PhotoPath = $"/shipment-photos/{fileName}";
        await _context.SaveChangesAsync();

        return Ok("Fotoğraf başarıyla yüklendi.");
    }

    [Authorize(Roles = "Customer")]
    [HttpPut("{id}/accept-bid")]
    public async Task<IActionResult> AcceptBid(int id, [FromBody] int bidId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var shipment = await _context.Shipments
            .Include(s => s.Bids)
            .FirstOrDefaultAsync(s => s.Id == id && s.CustomerId == userId);

        if (shipment == null)
            return NotFound("İlan bulunamadı.");

        var bid = shipment.Bids.FirstOrDefault(b => b.Id == bidId);
        if (bid == null)
            return NotFound("Teklif bu ilana ait değil.");

        shipment.AcceptedBidId = bidId;
        shipment.Status = ShipmentStatus.InTransit; // Mark shipment as "In Transit"

        // Notify the carrier
        var notification = new Notification
        {
            UserId = bid.ShipperId,
            Message = "Teklifiniz kabul edildi. İlan yolda olarak işaretlendi.",
        };
        _context.Notifications.Add(notification);

        // Notify other bidders
        var otherBidders = shipment.Bids.Where(b => b.Id != bidId).Select(b => b.ShipperId).Distinct();
        foreach (var bidderId in otherBidders)
        {
            _context.Notifications.Add(new Notification
            {
                UserId = bidderId,
                Message = "İlan için başka bir teklif kabul edildi.",
            });
        }

        await _context.SaveChangesAsync();

        return Ok("Teklif kabul edildi ve ilan 'Yolda' olarak işaretlendi.");
    }

    [Authorize(Roles = "Customer")]
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateShipmentStatus(int id, [FromBody] ShipmentStatus status)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var shipment = await _context.Shipments
            .FirstOrDefaultAsync(s => s.Id == id && s.CustomerId == userId);

        if (shipment == null)
            return NotFound("İlan bulunamadı.");

        shipment.Status = status;

        // Trigger rating request if completed
        if (status == ShipmentStatus.Completed)
        {
            var notification = new Notification
            {
                UserId = shipment.AcceptedBid?.ShipperId ?? 0,
                Message = "İlan başarıyla tamamlandı. Lütfen bir değerlendirme bırakın.",
            };
            _context.Notifications.Add(notification);
        }

        await _context.SaveChangesAsync();

        return Ok($"İlan durumu '{status}' olarak güncellendi.");
    }

    [Authorize(Roles = "Customer")]
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelShipment(int id, [FromBody] string reason)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var shipment = await _context.Shipments
            .FirstOrDefaultAsync(s => s.Id == id && s.CustomerId == userId);

        if (shipment == null)
            return NotFound("İlan bulunamadı.");

        if (shipment.Status == ShipmentStatus.Completed)
            return BadRequest("Tamamlanmış bir ilan iptal edilemez.");

        shipment.Status = ShipmentStatus.Cancelled;

        // Record cancellation reason
        var notification = new Notification
        {
            UserId = shipment.AcceptedBid?.ShipperId ?? 0,
            Message = $"İlan iptal edildi. Sebep: {reason}",
        };
        _context.Notifications.Add(notification);

        await _context.SaveChangesAsync();

        return Ok("İlan başarıyla iptal edildi.");
    }
}
