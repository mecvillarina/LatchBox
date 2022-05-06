using System;
using System.Numerics;

using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;

namespace LatchBoxVestingTokenVaultContract
{
    public partial class LatchBoxVestingTokenVaultContract : SmartContract
    {
        /// <summary>
        /// Validates if the tokenScriptHash is valid to be vested
        /// </summary>
        /// <param name="tokenScriptHash">Token Script Hash</param>
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

        private static void ValidateVestingPeriodParameterData(LatchBoxVestingPeriodParameter[] periods, BigInteger totalAmount)
        {
            BigInteger vestingtotalAmountCheck = 0;

            if(periods == null || periods.Length < 2) ReportErrorAndThrow("LatchBox Vesting MUST have at least 2 periods.");

            foreach (var period in periods)
            {
                BigInteger periodTotalAmountCheck = 0;

                if ((period.UnlockTime - Runtime.Time) < 86400000) ReportErrorAndThrow("The parameter date unlock MUST be at least 1 day.");

                if(period.Receivers == null || period.Receivers.Length == 0) ReportErrorAndThrow("Every period MUST have at least one receiver.");

                foreach(var receiver in period.Receivers)
                {
                    if (!ValidateAddress(receiver.Address))
                    {
                        ReportErrorAndThrow("Receiver addresses SHOULD be 20-byte non-zero addresses.");
                    }
                    else if (ContractManagement.GetContract(receiver.Address) != null)
                    {
                        ReportErrorAndThrow("Receiver must not be a contract.");
                    }
                    if (receiver.Amount <= 0)
                    {
                        ReportErrorAndThrow("All receiver amount must be greater than 0.");
                    }

                    vestingtotalAmountCheck += receiver.Amount;
                    periodTotalAmountCheck += receiver.Amount;
                }

                if(periodTotalAmountCheck != period.TotalAmount)
                {
                    ReportErrorAndThrow("The Period Total Amount is not equal to the summation receivers' amounts on that period.");
                }
            }

            if (vestingtotalAmountCheck != totalAmount)
            {
                ReportErrorAndThrow("The total amount is not equal to the summation receivers' amounts.");
            }
        }

        private static bool ValidateAddress(UInt160 address) => address.IsValid && !address.IsZero;

        private static void ReportErrorAndThrow(string errMsg)
        {
            var message = $"[{nameof(LatchBoxVestingTokenVaultContract)}] {errMsg}";
            Runtime.Log(message);
            throw new Exception(message);
        }
    }
}