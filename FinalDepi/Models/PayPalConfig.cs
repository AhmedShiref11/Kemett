namespace FinalDepi.Models
{
    public class PayPalConfig
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Mode { get; set; } // "sandbox" or "live"
    }
} 