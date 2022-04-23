using Client.Infrastructure.Extensions;
using Client.Infrastructure.Models;

namespace Client.Models
{
    public class AssetCounterModel
    {
        public AssetToken AssetToken { get; private set; }
        public AssetCounter AssetCounter { get; private set; }
        public string LockedAmountDisplay { get; private set; }
        public string UnlockedAmountDisplay { get; private set; }

        public AssetCounterModel(AssetCounter counter)
        {
            AssetCounter = counter;
        }

        public void SetAssetToken(AssetToken assetToken)
        {
            AssetToken = assetToken;
            LockedAmountDisplay = $"{AssetCounter.LockedAmount.ToAmount(AssetToken.Decimals).ToAmountDisplay(AssetToken.Decimals)} {AssetToken.Symbol}";
            UnlockedAmountDisplay = $"{AssetCounter.UnlockedAmount.ToAmount(AssetToken.Decimals).ToAmountDisplay(AssetToken.Decimals)} {AssetToken.Symbol}";
        }
    }
}
