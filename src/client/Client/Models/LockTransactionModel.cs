using Client.Infrastructure.Constants;
using Client.Infrastructure.Extensions;
using Client.Infrastructure.Models;
using MudBlazor;
using System.Numerics;

namespace Client.Models
{
    public class LockTransactionModel
    {
        private readonly BigInteger _totalAmount;

        public AssetToken AssetToken { get; private set; }
        public LockTransaction Transaction { get; private set; }
        public string InitiatorAddressDisplay { get; private set; }
        public string TotalAmountDisplay { get; private set; }
        public string DateStartDisplay { get; private set; }
        public string DateUnlockDisplay { get; private set; }
        public string RevocableDisplay { get; private set; }
        public string StatusDisplay { get; private set; }
        public Color StatusDisplayColor { get; private set; }

        public LockTransactionModel(LockTransaction transaction, AssetToken assetToken)
        {
            Transaction = transaction;
            AssetToken = assetToken;

            foreach (var receiver in transaction.Receivers)
            {
                _totalAmount += receiver.Amount;
            }

            InitiatorAddressDisplay = Transaction.InitiatorAddress;
            DateStartDisplay = Transaction.StartTime.ToString(ClientConstants.LongDateTimeFormat);
            DateUnlockDisplay = Transaction.UnlockTime.ToString(ClientConstants.LongDateTimeFormat);
            RevocableDisplay = Transaction.IsRevocable ? "Yes" : "No";
            TotalAmountDisplay = $"{_totalAmount.ToAmount(AssetToken.Decimals).ToAmountDisplay(AssetToken.Decimals)} {AssetToken.Symbol}";

            if (Transaction.IsActive)
            {
                if (DateTime.UtcNow < Transaction.UnlockTime)
                {
                    StatusDisplay = "Locked";
                    StatusDisplayColor = Color.Primary;
                }
                else
                {
                    StatusDisplay = "Unlocked";
                    StatusDisplayColor = Color.Info;
                }
            }
            else if (Transaction.IsRevoked)
            {
                StatusDisplay = "Revoked";
                StatusDisplayColor = Color.Error;
            }
            else
            {
                StatusDisplay = "Claimed";
                StatusDisplayColor = Color.Info;
            }
        }
    }
}
