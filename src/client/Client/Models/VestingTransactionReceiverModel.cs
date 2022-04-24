using Client.Infrastructure.Extensions;
using Client.Infrastructure.Models;
using MudBlazor;

namespace Client.Models
{
    public class VestingTransactionReceiverModel
    {
        public AssetToken AssetToken { get; private set; }
        public VestingTransaction Transaction { get; private set; }
        public VestingPeriod Period { get; set; }
        public VestingReceiver Receiver { get; set; }
        public string StatusDisplay { get; private set; }
        public Color StatusColor { get; private set; }

        public bool CanClaim { get; private set; }
        public string AmountDisplay { get; private set; }

        public VestingTransactionReceiverModel(VestingTransaction transaction, VestingReceiver receiver)
        {
            Transaction = transaction;

            Period = transaction.Periods.First(x => x.PeriodIndex == receiver.PeriodIndex);
            Receiver = receiver;

            if (Receiver.DateRevoked.HasValue)
            {
                StatusDisplay = "Revoked";
                StatusColor = Color.Error;
            }
            else if (Receiver.DateClaimed.HasValue)
            {
                StatusDisplay = "Claimed";
                StatusColor = Color.Info;
            }
            else
            {
                if (Transaction.IsActive)
                {
                    if (DateTime.UtcNow < Period.UnlockTime)
                    {
                        StatusDisplay = "Locked";
                        StatusColor = Color.Primary;
                    }
                    else
                    {
                        StatusDisplay = "Unlocked";
                        StatusColor = Color.Info;
                        CanClaim = true;
                    }
                }
            }
        }

        public void SetAssetToken(AssetToken assetToken)
        {
            AssetToken = assetToken;
            AmountDisplay = $"{Receiver.Amount.ToAmount(AssetToken.Decimals).ToAmountDisplay(AssetToken.Decimals)} {AssetToken.Symbol}";
        }
    }
}
