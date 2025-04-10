using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using NakliyeApp.Data;
using NakliyeApp.DTOs;
using NakliyeApp.Hubs;
using NakliyeApp.Models;
using System.Security.Claims;

namespace NakliyeApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatController(ApplicationDbContext context, IHubContext<ChatHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] ChatMessageDto message)
    {
        // Broadcast the message using SignalR
        await _hubContext.Clients.User(message.RecipientId.ToString())
            .SendAsync("ReceiveMessage", message.SenderId, message.Content);

        // Save the message as a notification
        var notification = new Notification
        {
            UserId = message.RecipientId,
            Message = $"Yeni bir mesaj aldınız: {message.Content}",
            CreatedAt = DateTime.UtcNow
        };
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        return Ok("Mesaj başarıyla gönderildi.");
    }

    [HttpGet("history/{userId}")]
    public async Task<IActionResult> GetChatHistory(int userId)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Retrieve chat history between the current user and the specified user
        var messages = await _context.Notifications
            .Where(n => (n.UserId == currentUserId && n.CreatedByUserId == userId) ||
                        (n.UserId == userId && n.CreatedByUserId == currentUserId))
            .OrderBy(n => n.CreatedAt)
            .ToListAsync();

        return Ok(messages);
    }
}
