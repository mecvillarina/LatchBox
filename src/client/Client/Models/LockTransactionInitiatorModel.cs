using Client.Infrastructure.Extensions;
using Client.Infrastructure.Models;
using MudBlazor;
using System.Numerics;

namespace Client.Models
{
    public class LockTransactionInitiatorModel
    {
        public AssetToken AssetToken { get; private set; }
        public LockTransaction Transaction { get; private set; }
        public BigInteger TotalAmount { get; private set; }
        public string TotalAmountDisplay { get; private set; }

        public string Status { get; private set; }
        public Color StatusColor { get; private set; }

        public bool IsRevocable { get; private set; }

        public LockTransactionInitiatorModel(LockTransaction transaction)
        {
            Transaction = transaction;

            foreach (var receiver in transaction.Receivers)
            {
                TotalAmount += receiver.Amount;
            }

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
                }
            }
            else if (Transaction.IsRevoked)
            {
                Status = "Revoked";
                StatusColor = Color.Error;
            }
            else
            {
                Status = "Claimed";
                StatusColor = Color.Info;
            }

            IsRevocable = Transaction.IsRevocable;
        }

        public void SetAssetToken(AssetToken assetToken)
        {
            AssetToken = assetToken;
            TotalAmountDisplay = $"{TotalAmount.ToAmount(AssetToken.Decimals).ToAmountDisplay(AssetToken.Decimals)} {AssetToken.Symbol}";
        }
    }
}
