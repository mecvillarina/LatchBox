using System;
using System.Numerics;

using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;

namespace LatchBoxLockTokenVaultContract
{
    public partial class LatchBoxLockTokenVaultContract : SmartContract
    {
        /// <summary>
        /// Validates if the tokenScriptHash is valid to be locked
        /// </summary>
        /// <param name="tokenScriptHash"></param>
        /// <exception cref="System.Exception">Thrown when the tokenScriptHash is not a NEP-17 Token and if the NEP-17 Token doesn't have onNEP17Payment permission.</exception>
        public static void ValidateNEP17Token(UInt160 tokenScriptHash)
        {
            if (ValidateAddress(tokenScriptHash))
            {
                var contract = ContractManagement.GetContract(tokenScriptHash);

                if (contract != null)
                {
                    foreach (var supportedStandard in contract.Manifest.SupportedStandards)
                    {
                        if (supportedStandard == "NEP-17")
                        {
                            foreach (var permission in contract.Manifest.Permissions)
                            {
                                if (permission.Contract == null && permission.Methods == null)
                                    return;

                                foreach (var method in permission.Methods)
                                {
                                    if ((permission.Contract == null || permission.Contract == Runtime.ExecutingScriptHash) && method == "onNEP17Payment")
                                        return;
                                }
                            }
                        }
                    }
                }

            }

            ReportErrorAndThrow("Only supports NEP-17 Tokens and SHOULD have permission for onNEP17Payment or wildcard method.");
        }

        private static void ValidateLockReceiverParameterData(LatchBoxLockReceiverParameter[] receivers, BigInteger totalAmount)
        {
            BigInteger totalAmountCheck = 0;

            if(receivers == null || receivers.Length == 0) ReportErrorAndThrow("LatchBox Lock MUST have at least one receiver.");

            foreach (var receiver in receivers)
            {
                if (!ValidateAddress(receiver.ReceiverAddress))
                {
                    ReportErrorAndThrow("Receiver addresses SHOULD be 20-byte non-zero addresses.");
                }
                else if (ContractManagement.GetContract(receiver.ReceiverAddress) != null)
                {
                    ReportErrorAndThrow("Receiver must not be a contract.");
                }

                if (receiver.Amount <= 0)
                {
                    ReportErrorAndThrow("All receiver amount must be greater than 0.");
                }

                totalAmountCheck += receiver.Amount;
            }

            if (totalAmountCheck != totalAmount)
            {
                ReportErrorAndThrow("The total amount is not equal to the summation receivers' amounts.");
            }
        }

        private static bool ValidateAddress(UInt160 address) => address.IsValid && !address.IsZero;

        private static void ReportErrorAndThrow(string errMsg)
        {
            var message = $"[{nameof(LatchBoxLockTokenVaultContract)}] {errMsg}";
            Runtime.Log(message);
            throw new Exception(message);
        }

    }
}