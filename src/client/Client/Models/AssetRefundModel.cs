using Client.Infrastructure.Models;

namespace Client.Models
{
    public class AssetRefundModel
    {
        public AssetToken AssetToken { get; private set; }
        public string WalletAddress { get; private set; }
        public AssetRefund AssetRefund { get; private set; }

        public AssetRefundModel(string walletAddress, AssetRefund refund)
        {
            WalletAddress = walletAddress;
            AssetRefund = refund;
        }

        public void SetAssetToken(AssetToken assetToken)
        {
            AssetToken = assetToken;
        }
    }
}
