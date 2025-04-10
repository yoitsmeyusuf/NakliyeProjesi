using Microsoft.AspNetCore.SignalR;
using NakliyeApp.Data;
using NakliyeApp.Models;

namespace NakliyeApp.Hubs;

public class ChatHub : Hub
{
    private readonly ApplicationDbContext _context;

    public ChatHub(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SendMessage(string recipientId, string senderId, string message)
    {
        // Save the message to the database (optional)
        var notification = new Notification
        {
            UserId = int.Parse(recipientId),
            Message = $"Yeni bir mesaj aldınız: {message}",
            CreatedAt = DateTime.UtcNow
        };
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        // Send the message to the recipient
        await Clients.User(recipientId).SendAsync("ReceiveMessage", senderId, message);

        // Send a notification to the recipient
        await Clients.User(recipientId).SendAsync("ReceiveNotification", notification.Message);
    }
}
