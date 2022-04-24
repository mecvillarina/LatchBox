using Client.Infrastructure.Extensions;
using Client.Infrastructure.Models;
using MudBlazor;
using System.Numerics;

namespace Client.Models
{
    public class VestingTransactionInitiatorModel
    {
        public AssetToken AssetToken { get; private set; }
        public VestingTransaction Transaction { get; private set; }
        public BigInteger TotalAmount { get; private set; }
        public string TotalAmountDisplay { get; private set; }
        public string PeriodDisplay { get; set; }
        public string StatusDisplay { get; private set; }
        public Color StatusColor { get; private set; }
        public bool IsRevocable { get; private set; }
        public VestingPeriod UpcomingPeriod { get; private set; } 
        public VestingTransactionInitiatorModel(VestingTransaction transaction)
        {
            Transaction = transaction;

            foreach (var receiver in transaction.Receivers)
            {
                TotalAmount += receiver.Amount;
            }

            if (Transaction.IsActive)
            {
                if (Transaction.Periods.Any(x => DateTime.UtcNow < x.UnlockTime))
                {
                    StatusDisplay = "On Vesting";
                    StatusColor = Color.Primary;
                }
                else
                {
                    StatusDisplay = "Unlocked";
                    StatusColor = Color.Info;
                }
            }
            else if (Transaction.IsRevoked)
            {
                StatusDisplay = "Revoked";
                StatusColor = Color.Error;
            }
            else
            {
                StatusDisplay = "Completed";
                StatusColor = Color.Info;
            }

            IsRevocable = Transaction.IsRevocable;
            PeriodDisplay = $"{transaction.Periods.Count} Periods";
            UpcomingPeriod = transaction.Periods.FirstOrDefault(x => x.UnlockTime > DateTime.UtcNow);
        }

        public void SetAssetToken(AssetToken assetToken)
        {
            AssetToken = assetToken;
            TotalAmountDisplay = $"{TotalAmount.ToAmount(AssetToken.Decimals).ToAmountDisplay(AssetToken.Decimals)} {AssetToken.Symbol}";
        }
    }
}
