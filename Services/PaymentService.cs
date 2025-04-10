using System.Security.Cryptography;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using NakliyeApp.Models;
using Microsoft.Extensions.Options;

namespace NakliyeApp.Services;

public class PaymentService
{
    private readonly PayTRSettings _payTRSettings;


    public PaymentService(IOptions<PayTRSettings> payTRSettings)
    {
        _payTRSettings = payTRSettings.Value;
    }
 

    public async Task<PaymentResponse> ProcessPayment(decimal amount, string currency, string description, string cardNumber, string cardHolderName, string expireMonth, string expireYear, string cvc)
    {
        var merchantId = _payTRSettings.MerchantId;
        var merchantKey = _payTRSettings.MerchantKey;
        var merchantSalt = _payTRSettings.MerchantSalt;
        var baseUrl = _payTRSettings.BaseUrl;
        var callbackUrl = _payTRSettings.CallbackUrl;

        // Prepare the request data
         if (string.IsNullOrEmpty(merchantId) || string.IsNullOrEmpty(merchantKey) || string.IsNullOrEmpty(merchantSalt))
        {
            throw new ArgumentException("PayTR settings are not configured properly.");
        }
        if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(callbackUrl))
        {
            throw new ArgumentException("PayTR base URL or callback URL is not configured properly.");
        }
        
        var requestData = new Dictionary<string, string>
        {
            { "merchant_id", merchantId ?? throw new ArgumentNullException(nameof(merchantId)) },
            { "user_ip", "127.0.0.1" }, // Replace with the actual user's IP
            { "merchant_oid", Guid.NewGuid().ToString() },
            { "email", "email@example.com" }, // Replace with the actual user's email
            { "payment_amount", ((int)(amount * 100)).ToString() }, // Convert to cents
            { "currency", currency ?? "TRY" },
            { "test_mode", "1" }, // Set to "0" for production
            { "non_3d", "0" }, // Set to "1" for non-3D secure payments
            { "merchant_ok_url", $"{baseUrl}/payment-success" },
            { "merchant_fail_url", $"{baseUrl}/payment-fail" },
            { "user_name", cardHolderName ?? throw new ArgumentNullException(nameof(cardHolderName)) },
            { "card_number", cardNumber ?? throw new ArgumentNullException(nameof(cardNumber)) },
            { "card_expiry_month", expireMonth ?? throw new ArgumentNullException(nameof(expireMonth)) },
            { "card_expiry_year", expireYear ?? throw new ArgumentNullException(nameof(expireYear)) },
            { "card_cvc", cvc ?? throw new ArgumentNullException(nameof(cvc)) }
        };

        // Generate the PayTR token
        var tokenString = string.Join("", requestData.Values) + merchantSalt;
        using var sha256 = SHA256.Create();
        var tokenBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(tokenString));
        var token = Convert.ToBase64String(tokenBytes);

        requestData.Add("paytr_token", token);

         using var httpClient = new HttpClient();
    var content = new FormUrlEncodedContent(requestData);
    content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

    var response = await httpClient.PostAsync($"{baseUrl}/payment", content);
    var responseString = await response.Content.ReadAsStringAsync();

    var responseData = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);
    
    if (responseData == null)
    {
        return new PaymentResponse 
        { 
            ErrorMessage = "Invalid response from payment gateway" 
        };
    }

    // 3D Secure yönlendirmesi gerekiyorsa
    if (responseData.TryGetValue("status", out var status) && status == "redirect" && 
        responseData.ContainsKey("redirect_url"))
    {
        return new PaymentResponse
        {
            RequiresRedirect = true,
            RedirectUrl = responseData["redirect_url"]
        };
    }

    // Başarılı ödeme
    if (status == "success" && responseData.ContainsKey("payment_id"))
    {
        return new PaymentResponse
        {
            PaymentId = responseData["payment_id"]
        };
    }

    // Hata durumu
    return new PaymentResponse
    {
        ErrorMessage = responseData.TryGetValue("reason", out var reason) ? 
                       reason : "Unknown error occurred"
    };
}

}