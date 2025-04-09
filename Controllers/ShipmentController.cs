using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NakliyeApp.Data;
using NakliyeApp.Models;
using System.Security.Claims;

namespace NakliyeApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShipmentController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ShipmentController(ApplicationDbContext context)
    {
        _context = context;
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
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var userType = User.FindFirstValue(ClaimTypes.Role);

        if (userType != UserType.Customer.ToString())
            return Forbid("Sadece ilan veren kullanıcılar ilan oluşturabilir.");

        shipment.CustomerId = userId;
        shipment.PickupDate = shipment.PickupDate.ToUniversalTime();

        _context.Shipments.Add(shipment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetShipment), new { id = shipment.Id }, shipment);
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
    public async Task<IActionResult> AcceptBid(int shipmentId, int bidId)
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
        await _context.SaveChangesAsync();

        return Ok("Teklif kabul edildi.");
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
        await _context.SaveChangesAsync();

        return Ok("İş başarıyla tamamlandı.");
    }
    


}
