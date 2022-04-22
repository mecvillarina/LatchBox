using Client.Infrastructure.Extensions;
using Client.Infrastructure.Models;
using MudBlazor;

namespace Client.Models
{
    public class LockTransactionReceiverModel
    {
        public AssetToken AssetToken { get; private set; }
        public LockTransaction Transaction { get; private set; }
        public LockReceiver Receiver { get; set; }
        public string Status { get; private set; }
        public Color StatusColor { get; private set; }

        public bool CanClaim { get; private set; }
        public string AmountDisplay { get; set; }

        public LockTransactionReceiverModel(string receiverAddress, LockTransaction transaction)
        {
            Transaction = transaction;

            Receiver = transaction.Receivers.First(x => x.ReceiverAddress == receiverAddress);

            if (Receiver.DateRevoked.HasValue)
            {
                Status = "Revoked";
                StatusColor = Color.Error;
            }
            else if (Receiver.DateClaimed.HasValue)
            {
                Status = "Claimed";
                StatusColor = Color.Info;
            }
            else
            {
                if (Transaction.IsActive)
                {
                    if (DateTime.UtcNow < Transaction.UnlockTime)
                    {
                        Status = "Locked";
                        StatusColor = Color.Primary;
                    }
                    else
                    {
                        Status = "Unlocked";
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
