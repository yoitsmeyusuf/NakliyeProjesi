using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NakliyeApp.Data;
using NakliyeApp.Models;
using NakliyeApp.Services;
using System.Security.Claims;

namespace NakliyeApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RatingController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly RatingService _ratingService;

    public RatingController(ApplicationDbContext context, RatingService ratingService)
    {
        _context = context;
        _ratingService = ratingService;
    }

    [HttpPost]
    public async Task<IActionResult> AddRating([FromBody] Rating rating)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (await _ratingService.HasUserRatedShipment(rating.UserId, rating.ShipmentId))
            return BadRequest("Bu kullanıcı için bu ilan üzerinden zaten bir puan verdiniz.");

        rating.CreatedByUserId = userId;
        await _ratingService.AddRatingAsync(rating);

        return Ok("Puan başarıyla eklendi.");
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserRatings(int userId)
    {
        var ratings = await _ratingService.GetRatingsForUserAsync(userId);
        return Ok(ratings);
    }
}
