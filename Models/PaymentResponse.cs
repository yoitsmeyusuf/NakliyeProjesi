namespace NakliyeApp.Models;


    public class PaymentResponse
    {
        public string? PaymentId { get; set; }
        public string? RedirectUrl { get; set; }
        public bool RequiresRedirect { get; set; } = false;
        public string? ErrorMessage { get; set; }
    }
