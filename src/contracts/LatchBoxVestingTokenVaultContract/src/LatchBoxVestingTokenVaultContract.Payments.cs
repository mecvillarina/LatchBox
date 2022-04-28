using System.Numerics;

using Neo;
using Neo.SmartContract;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Services;

namespace LatchBoxVestingTokenVaultContract
{
    public partial class LatchBoxVestingTokenVaultContract : SmartContract
    {
        private const byte Prefix_PaymentTokenScriptHash = 0xAA;
        private const byte Prefix_PaymentTokenAddVestingFee = 0xAB;
        private const byte Prefix_PaymentTokenClaimVestingFee = 0xAC;
        private const byte Prefix_PaymentTokenRevokeVestingFee = 0xAD;
        private const byte Prefix_PaymentTokenBurnedAmount = 0xAE;
 
        [InitialValue("Nh8heGKWEYzrrRcP4zrQ9D4LBCPf7otoZG", ContractParameterType.Hash160)]
        static readonly UInt160 FoundationAddress = default!;

        [InitialValue("Ngp1rgABHPbvXprFRtNyBzVGro6JChudYY", ContractParameterType.Hash160)]
        static readonly UInt160 StakingAddress = default!;

        [InitialValue("NWN1iyBuSpsUhvLhLddo9tbU8fYEaLp4A4", ContractParameterType.Hash160)]
        static readonly UInt160 PlatformAddress = default!;

        [Safe]
        public static UInt160 GetPaymentTokenScriptHash() => (UInt160)Storage.Get(Storage.CurrentContext, new byte[] { Prefix_PaymentTokenScriptHash });

        [Safe]
        public static BigInteger GetPaymentTokenAddVestingFee() => (BigInteger)Storage.Get(Storage.CurrentContext, new byte[] { Prefix_PaymentTokenAddVestingFee });

        [Safe]
        public static BigInteger GetPaymentTokenClaimVestingFee() => (BigInteger)Storage.Get(Storage.CurrentContext, new byte[] { Prefix_PaymentTokenClaimVestingFee });

        [Safe]
        public static BigInteger GetPaymentTokenRevokeVestingFee() => (BigInteger)Storage.Get(Storage.CurrentContext, new byte[] { Prefix_PaymentTokenRevokeVestingFee });

        [Safe]
        public static BigInteger GetPaymentBurnedAmount() => (BigInteger)Storage.Get(Storage.CurrentContext, new byte[] { Prefix_PaymentTokenBurnedAmount });

        public static void SetupPayment(UInt160 tokenScriptHash, BigInteger addVestingFee, BigInteger claimVestingFee, BigInteger revokeVestingFee) 
        {
            if (!IsOwner()) ReportErrorAndThrow("No authorization.");

            ValidateNEP17Token(tokenScriptHash);

            if(addVestingFee < 0) ReportErrorAndThrow("Add LatchBox Vesting Fee must be greater than or equal to 0.");
            if(claimVestingFee < 0) ReportErrorAndThrow("Claim LatchBox Vesting Fee must be greater than or equal to 0.");
            if(revokeVestingFee < 0) ReportErrorAndThrow("Revoke LatchBox Vesting Fee must be greater than or equal to 0.");

            Storage.Put(Storage.CurrentContext, new byte[] { Prefix_PaymentTokenScriptHash }, tokenScriptHash);
            Storage.Put(Storage.CurrentContext, new byte[] { Prefix_PaymentTokenAddVestingFee }, addVestingFee);
            Storage.Put(Storage.CurrentContext, new byte[] { Prefix_PaymentTokenClaimVestingFee }, claimVestingFee);
            Storage.Put(Storage.CurrentContext, new byte[] { Prefix_PaymentTokenRevokeVestingFee }, revokeVestingFee);
        }

        private static void UpdatePaymentBurnAmount(BigInteger value) 
        {
            var amount = (BigInteger)Storage.Get(Storage.CurrentContext, new byte[] { Prefix_PaymentTokenBurnedAmount });
            Storage.Put(Storage.CurrentContext, new byte[] { Prefix_PaymentTokenBurnedAmount }, amount + value);
        }
    }

}