using Client.Infrastructure.Constants;
using Client.Infrastructure.Extensions;
using Client.Infrastructure.Models;
using MudBlazor;
using System.Numerics;

namespace Client.Models
{
    public class VestingTransactionModel
    {
        private readonly BigInteger _totalAmount;

        public AssetToken AssetToken { get; private set; }
        public VestingTransaction Transaction { get; private set; }
        public string InitiatorAddressDisplay { get; private set; }
        public string TotalAmountDisplay { get; private set; }
        public string DateCreationDisplay { get; private set; }
        public string RevocableDisplay { get; private set; }
        public string StatusDisplay { get; private set; }
        public Color StatusDisplayColor { get; private set; }

        public VestingTransactionModel(VestingTransaction transaction, AssetToken assetToken)
        {
            Transaction = transaction;
            AssetToken = assetToken;

            foreach (var receiver in transaction.Receivers)
            {
                _totalAmount += receiver.Amount;
            }

            InitiatorAddressDisplay = Transaction.InitiatorAddress;
            DateCreationDisplay = Transaction.CreationTime.ToString(ClientConstants.LongDateTimeFormat);
            RevocableDisplay = Transaction.IsRevocable ? "Yes" : "No";
            TotalAmountDisplay = $"{_totalAmount.ToAmount(AssetToken.Decimals).ToAmountDisplay(AssetToken.Decimals)} {AssetToken.Symbol}";

            if (Transaction.IsActive)
            {
                if (Transaction.Periods.Any(x => DateTime.UtcNow < x.UnlockTime))
                {
                    StatusDisplay = "On Vesting";
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
                StatusDisplay = "Completed";
                StatusDisplayColor = Color.Info;
            }

        }
    }
}
