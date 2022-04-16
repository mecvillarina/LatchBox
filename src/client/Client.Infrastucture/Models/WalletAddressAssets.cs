using System.Collections.Generic;

namespace Client.Infrastructure.Models
{
    public class WalletAddressAssets
    {
        public string Address { get; set; }
        public List<AssetToken> Assets { get; set; }
    }
}
