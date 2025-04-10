using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using NakliyeApp.Data;
using NakliyeApp.Models;
using NakliyeApp.Hubs;
using System.Security.Claims;

namespace NakliyeApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationController(ApplicationDbContext context, IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    // GET: api/notification
    [HttpGet]
    public async Task<IActionResult> GetNotifications()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return Ok(notifications);
    }

    // PUT: api/notification/mark-as-read/{id}
    [HttpPut("mark-as-read/{id}")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

        if (notification == null)
            return NotFound("Bildirim bulunamadı.");

        notification.IsRead = true;
        await _context.SaveChangesAsync();

        return Ok("Bildirim okundu olarak işaretlendi.");
    }

    // POST: api/notification/send
    [HttpPost("send")]
    public async Task<IActionResult> SendNotification([FromBody] Notification notification)
    {
        var user = await _context.Users.FindAsync(notification.UserId);
        if (user == null)
            return NotFound("Kullanıcı bulunamadı.");

        notification.CreatedAt = DateTime.UtcNow;
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        // Broadcast the notification using SignalR
        await _hubContext.Clients.User(notification.UserId.ToString())
            .SendAsync("ReceiveNotification", notification.Message);

        return Ok("Bildirim gönderildi.");
    }
}
