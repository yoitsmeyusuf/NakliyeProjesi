using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NakliyeApp.DTOs;
using NakliyeApp.Services;

namespace NakliyeApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly PaymentService _paymentService;

    public PaymentController(PaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("process")]
    public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequestDto dto)
    {
         var paymentResponse = await _paymentService.ProcessPayment(
        dto.Amount,
        dto.Currency,
        dto.Description,
        dto.CardNumber,
        dto.CardHolderName,
        dto.ExpireMonth,
        dto.ExpireYear,
        dto.Cvc
    );

    if (!string.IsNullOrEmpty(paymentResponse.ErrorMessage))
    {
        return BadRequest(paymentResponse.ErrorMessage);
    }

    if (paymentResponse.RequiresRedirect)
    {
        // Frontend'e yönlendirme URL'sini dön
        return Ok(new 
        { 
            redirectUrl = paymentResponse.RedirectUrl,
            requiresRedirect = true
        });
        
        // Veya doğrudan yönlendirme yapmak için:
        // return Redirect(paymentResponse.RedirectUrl);
    }

    // Ödeme başarılı
    return Ok(new { paymentId = paymentResponse.PaymentId });
    }
}
