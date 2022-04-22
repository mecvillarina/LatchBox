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

        public LockTransactionReceiverModel(string receiverAddress, LockTransaction transaction)
        {
            Transaction = transaction;

            Receiver = transaction.Receivers.First(x => x.ReceiverAddress == receiverAddress);

            if (Transaction.IsActive)
            {
                if (DateTime.UtcNow < Transaction.UnlockTime)
                {
                    Status = "Locked";
                    StatusColor = Color.Primary;
                }
                else if(!Receiver.DateClaimed.HasValue)
                {
                    Status = "Unlocked";
                    StatusColor = Color.Info;
                    CanClaim = true;
                }
                else
                {
                    Status = "Claimed";
                    StatusColor = Color.Info;
                }
            }
            else if (Receiver.DateRevoked.HasValue)
            {
                Status = "Revoked";
                StatusColor = Color.Error;
            }
            else
            {
                Status = "Claimed";
                StatusColor = Color.Info;
            }
        }

        public void SetAssetToken(AssetToken assetToken)
        {
            AssetToken = assetToken;
        }
    }
}
