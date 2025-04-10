namespace NakliyeApp.DTOs;

public class PaymentRequestDto
{
    public required decimal Amount { get; set; }
    public required string Currency { get; set; } = "TRY";
    public required string Description { get; set; }
    public required string CardNumber { get; set; }
    public required string CardHolderName { get; set; }
    public required string ExpireMonth { get; set; }
    public required string ExpireYear { get; set; }
    public required string Cvc { get; set; }
}
