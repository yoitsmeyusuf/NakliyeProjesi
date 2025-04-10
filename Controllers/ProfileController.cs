using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NakliyeApp.Data;
using NakliyeApp.DTOs;
using NakliyeApp.Models;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProfileController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/profile
    [HttpGet]
    public async Task<ActionResult<AppUser>> GetProfile()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return NotFound("Kullanıcı bulunamadı.");

        return Ok(new
        {
            user.Id,
            user.FullName,
            user.Email,
            user.PhoneNumber,
            user.UserType
        });
    }

    // PUT: api/profile
    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return NotFound("Kullanıcı bulunamadı.");

        user.FullName = dto.FullName;
        user.PhoneNumber = dto.PhoneNumber;

        await _context.SaveChangesAsync();

        return Ok("Profil güncellendi.");
    }
    // PUT: api/profile/change-password
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return NotFound("Kullanıcı bulunamadı.");

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash);
        if (!isPasswordValid)
            return BadRequest("Mevcut şifre yanlış.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _context.SaveChangesAsync();

        return Ok("Şifre başarıyla değiştirildi.");
    }

    // GET: api/profile/{id}
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserProfile(int id)
    {
        var user = await _context.Users
            .Include(u => u.Bids)
            .Include(u => u.Shipments)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return NotFound("Kullanıcı bulunamadı.");

        var averageRating = user.Bids.Any() ? user.Bids.Average(b => b.Price) : 0;

        return Ok(new
        {
            user.Id,
            user.FullName,
            user.Email,
            user.UserType,
            AverageRating = averageRating
        });
    }

    // POST: api/profile/upload-photo
    [HttpPost("upload-photo")]
    public async Task<IActionResult> UploadProfilePhoto(IFormFile photo)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return NotFound("Kullanıcı bulunamadı.");

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "profile-photos");
        Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid()}_{photo.FileName}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await photo.CopyToAsync(stream);
        }

        user.PhotoPath = $"/profile-photos/{fileName}";
        await _context.SaveChangesAsync();

        return Ok("Profil fotoğrafı başarıyla yüklendi.");
    }
}
