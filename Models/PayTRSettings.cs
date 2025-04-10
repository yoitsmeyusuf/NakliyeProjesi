
namespace NakliyeApp.Models
{
    public class PayTRSettings
    {
        public string MerchantId { get; set; } = null!;
        public string MerchantKey { get; set; } = null!;
        public string MerchantSalt { get; set; } = null!;
        public string BaseUrl { get; set; } = null!;
        public string CallbackUrl { get; set; } = null!;
    }
}