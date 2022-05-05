using System.Numerics;

using Neo;
using Neo.SmartContract;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Services;

namespace LatchBoxLockTokenVaultContract
{
    public partial class LatchBoxLockTokenVaultContract : SmartContract
    {
        private const byte Prefix_PaymentTokenScriptHash = 0xAA;
        private const byte Prefix_PaymentTokenAddLockFee = 0xAB;
        private const byte Prefix_PaymentTokenClaimLockFee = 0xAC;
        private const byte Prefix_PaymentTokenRevokeLockFee = 0xAD;
        private const byte Prefix_PaymentTokenBurnedAmount = 0xAE;
 
        [InitialValue("Nh8heGKWEYzrrRcP4zrQ9D4LBCPf7otoZG", ContractParameterType.Hash160)]
        static readonly UInt160 FoundationAddress = default!;

        [InitialValue("Ngp1rgABHPbvXprFRtNyBzVGro6JChudYY", ContractParameterType.Hash160)]
        static readonly UInt160 StakingAddress = default!;

        [InitialValue("NWN1iyBuSpsUhvLhLddo9tbU8fYEaLp4A4", ContractParameterType.Hash160)]
        static readonly UInt160 PlatformAddress = default!;

        /// <summary>
        /// Returns the fee token script hash
        /// </summary>
        /// <returns></returns>
        [Safe]
        public static UInt160 GetPaymentTokenScriptHash() => (UInt160)Storage.Get(Storage.CurrentContext, new byte[] { Prefix_PaymentTokenScriptHash });

        /// <summary>
        /// Returns the fee for adding a lock
        /// </summary>
        /// <returns></returns>
        [Safe]
        public static BigInteger GetPaymentTokenAddLockFee() => (BigInteger)Storage.Get(Storage.CurrentContext, new byte[] { Prefix_PaymentTokenAddLockFee });

        /// <summary>
        /// Returns the fee for claiming a lock
        /// </summary>
        /// <returns></returns>
        [Safe]
        public static BigInteger GetPaymentTokenClaimLockFee() => (BigInteger)Storage.Get(Storage.CurrentContext, new byte[] { Prefix_PaymentTokenClaimLockFee });

        /// <summary>
        /// Returns the fee for revoking a lock
        /// </summary>
        /// <returns></returns>
        [Safe]
        public static BigInteger GetPaymentTokenRevokeLockFee() => (BigInteger)Storage.Get(Storage.CurrentContext, new byte[] { Prefix_PaymentTokenRevokeLockFee });

        /// <summary>
        /// Returns the total amount of fee token burned from the lock transactions
        /// </summary>
        /// <returns></returns>
        [Safe]
        public static BigInteger GetPaymentBurnedAmount() => (BigInteger)Storage.Get(Storage.CurrentContext, new byte[] { Prefix_PaymentTokenBurnedAmount });

        /// <summary>
        /// Setup the payment
        /// </summary>
        /// <param name="tokenScriptHash">NEP-17 Token, it could be NEO, GAS or any NEP-17 token</param>
        /// <param name="addLockFee">Add Lock Fee</param>
        /// <param name="claimLockFee">Claim Lock Fee</param>
        /// <param name="revokeLockFee">Revoke Lock Fee</param>
        public static void SetupPayment(UInt160 tokenScriptHash, BigInteger addLockFee, BigInteger claimLockFee, BigInteger revokeLockFee) 
        {
            if (!IsOwner()) ReportErrorAndThrow("No authorization.");

            ValidateNEP17Token(tokenScriptHash);

            if(addLockFee < 0) ReportErrorAndThrow("Add LatchBox Lock Fee must be greater than or equal to 0.");
            if(claimLockFee < 0) ReportErrorAndThrow("Claim LatchBox Lock Fee must be greater than or equal to 0.");
            if(revokeLockFee < 0) ReportErrorAndThrow("Revoke LatchBox Lock Fee must be greater than or equal to 0.");

            Storage.Put(Storage.CurrentContext, new byte[] { Prefix_PaymentTokenScriptHash }, tokenScriptHash);
            Storage.Put(Storage.CurrentContext, new byte[] { Prefix_PaymentTokenAddLockFee }, addLockFee);
            Storage.Put(Storage.CurrentContext, new byte[] { Prefix_PaymentTokenClaimLockFee }, claimLockFee);
            Storage.Put(Storage.CurrentContext, new byte[] { Prefix_PaymentTokenRevokeLockFee }, revokeLockFee);
        }

        private static void UpdatePaymentBurnAmount(BigInteger value) 
        {
            var amount = (BigInteger)Storage.Get(Storage.CurrentContext, new byte[] { Prefix_PaymentTokenBurnedAmount });
            Storage.Put(Storage.CurrentContext, new byte[] { Prefix_PaymentTokenBurnedAmount }, amount + value);
        }
    }

}