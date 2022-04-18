namespace Client.Infrastructure.Models
{
    public class PlatformTokenSaleInfo
    {
        public bool IsTokenOnSale { get; set; }
        public decimal TokensPerNEO { get; set; }
        public decimal TokensPerGAS { get; set; }
    }
}
