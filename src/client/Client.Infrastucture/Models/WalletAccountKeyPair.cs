using Neo.Wallets;

namespace Client.Infrastructure.Models
{
    public class WalletAccountKeyPair
    {
        public WalletAccount Account { get; set; }
        public KeyPair KeyPair { get; set; }
    }
}
