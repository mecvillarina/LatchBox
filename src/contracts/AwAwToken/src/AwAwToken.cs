using System;
using System.ComponentModel;
using System.Numerics;

using Neo;
using Neo.SmartContract;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;

namespace AwAwToken
{
    [DisplayName("AwAwToken")]
    [ManifestExtra("Author", "Mark Erwin Villarina")]
    [ManifestExtra("Description", "AwAwToken")]
    [ContractPermission("*", "onNEP17Payment")]
    [SupportedStandards("NEP-17")]
    public class AwAwToken : SmartContract
    {
        static readonly ulong InitialSupply = 10_000_000_000_000_000;

        [InitialValue("NcNEWCmr2xCi5Pkw1TKgVtYc1qEn6RDT9g", ContractParameterType.Hash160)]
        static readonly UInt160 Owner = default!;

        [DisplayName("Transfer")]
        public static event Action<UInt160, UInt160, BigInteger> OnTransfer = default!;

        [DisplayName("Burn")]
        public static event Action<UInt160, BigInteger> OnBurn = default!;
       
        private static Transaction Tx => (Transaction)Runtime.ScriptContainer;

        public static bool Verify() => IsOwner();

        public static string Symbol() => "AWAW";

        public static ulong Decimals() => 8;

        public static BigInteger TotalSupply() => TotalSupplyStorage.Get();
        
        private static bool ValidateAddress(UInt160 address) => address.IsValid && !address.IsZero;
        private static bool IsDeployed(UInt160 address) => ContractManagement.GetContract(address) != null;

        public static BigInteger BalanceOf(UInt160 account)
        {
            if (!ValidateAddress(account)) throw new Exception("The parameter account SHOULD be a 20-byte non-zero address.");
            return AssetStorage.Get(account);
        }

        public static bool Transfer(UInt160 from, UInt160 to, BigInteger amount, object data)
        {
            if (!ValidateAddress(from) || !ValidateAddress(to)) throw new Exception("The parameters from and to SHOULD be 20-byte non-zero addresses.");
            if (amount <= 0) throw new Exception("The parameter amount MUST be greater than 0.");
            if (!Runtime.CheckWitness(from) && !from.Equals(Runtime.CallingScriptHash)) throw new Exception("No authorization.");
            if (AssetStorage.Get(from) < amount) throw new Exception($"Insufficient {Symbol()} Token balance.");
            if (from == to) return true;

            AssetStorage.Reduce(from, amount);
            AssetStorage.Increase(to, amount);

            OnTransfer(from, to, amount);

            if (IsDeployed(to))
            {
                Contract.Call(to, "onNEP17Payment", CallFlags.None, new object[] { from, amount, data });
            }

            return true;
        }

        public static void Burn(UInt160 from, BigInteger amount, object data) 
        {
            if (!ValidateAddress(from)) throw new Exception("The parameters from SHOULD be 20-byte non-zero addresses.");
            if (amount <= 0) throw new Exception("The parameter amount MUST be greater than 0.");
            if (!Runtime.CheckWitness(from) && !from.Equals(Runtime.CallingScriptHash)) throw new Exception("No authorization.");
            if (AssetStorage.Get(from) < amount) throw new Exception($"Insufficient {Symbol()} Token balance.");

            AssetStorage.Reduce(from, amount);
            TotalSupplyStorage.Reduce(amount);
            
            OnBurn(from, amount);
        }

        public static void _deploy(object data, bool update)
        {
            if (update) return;

            if (TotalSupplyStorage.Get() > 0) throw new Exception("Contract has been deployed.");

            TotalSupplyStorage.Increase(InitialSupply);
            AssetStorage.Increase(Owner, InitialSupply);

            OnTransfer(UInt160.Zero, Owner, InitialSupply);
        }

        public static void Update(ByteString nefFile, string manifest)
        {
            if (!IsOwner()) throw new Exception("No authorization.");
            ContractManagement.Update(nefFile, manifest, null);
        }

        public static void Destroy()
        {
            if (!IsOwner()) throw new Exception("No authorization.");
            ContractManagement.Destroy();
        }

        private static bool IsOwner() => Runtime.CheckWitness(Owner);
    }
}
