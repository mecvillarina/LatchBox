using System;
using System.ComponentModel;
using System.Numerics;

using Neo;
using Neo.SmartContract;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;

namespace LatchBoxToken
{
    [DisplayName("LatchBoxToken")]
    [ManifestExtra("Author", "Mark Erwin Villarina")]
    [ManifestExtra("Description", "NEF-17 LatchBoxToken")]
    [ContractPermission("*", "onNEP17Payment")]
    [SupportedStandards("NEP-17")]
    public class LatchBoxToken : SmartContract
    {
        private static readonly ulong InitialSupply = 5_000_000_000_000_000;
        public static ulong MaxSupply() => 20_000_000_000_000_000;
        public static ulong TokensPerNEO() => 50_000_000_000;
        public static ulong TokensPerGAS() => 50;

        [InitialValue("NVh8ZCYi4rUsvBpMZgCb4gbm3bQVCMafWU", ContractParameterType.Hash160)]
        static readonly UInt160 Owner = default!;

        [DisplayName("Transfer")]
        public static event Action<UInt160, UInt160, BigInteger> OnTransfer = default!;

        [DisplayName("Burn")]
        public static event Action<UInt160, BigInteger> OnBurn = default!;
       
        private static Transaction Tx => (Transaction)Runtime.ScriptContainer;

        public static bool Verify() => IsOwner();

        public static string Symbol() => "LATCH";

        public static ulong Decimals() => 8;

        public static BigInteger TotalSupply() => TotalSupplyStorage.Get();
        
        public static bool IsTokenSaleEnabled() => AssetStorage.GetPaymentStatus();

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

        public static void OnNEP17Payment(UInt160 from, BigInteger amount, object data)
        {
            if (AssetStorage.GetPaymentStatus())
            {
                if (Runtime.CallingScriptHash == NEO.Hash)
                {
                    MintFromSale(amount * TokensPerNEO());
                }
                else if (Runtime.CallingScriptHash == GAS.Hash)
                {
                    if (from != null) MintFromSale(amount * TokensPerGAS());
                }
                else
                {
                    throw new Exception("Wrong calling script hash");
                }
            }
            else
            {
                throw new Exception("Payment is disable on this contract!");
            }
        }

        private static void MintFromSale(BigInteger amount)
        {
            var totalSupply = TotalSupplyStorage.Get();
            if (totalSupply <= 0) throw new Exception("Contract not deployed.");

            var avaliable_supply = MaxSupply() - totalSupply;

            if (amount <= 0) throw new Exception("Amount cannot be zero.");
            if (amount > avaliable_supply) throw new Exception("Insufficient supply for mint tokens.");

            AssetStorage.Increase(Tx.Sender, amount);
            TotalSupplyStorage.Increase(amount);

            OnTransfer(UInt160.Zero, Tx.Sender, amount);
        }

        public static void Mint(BigInteger amount)
        {
            if (!IsOwner()) throw new Exception("No authorization.");

            var totalSupply = TotalSupplyStorage.Get();
            if (totalSupply <= 0) throw new Exception("Contract not deployed.");

            var avaliable_supply = MaxSupply() - totalSupply;

            if (amount <= 0) throw new Exception("Amount cannot be zero.");
            if (amount > avaliable_supply) throw new Exception("Insufficient supply for mint tokens.");

            AssetStorage.Increase(Owner, amount);
            TotalSupplyStorage.Increase(amount);

            OnTransfer(UInt160.Zero, Owner, amount);
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

        public static void EnablePayment()
        {
            if (!IsOwner()) throw new Exception("No authorization.");
            AssetStorage.Enable();
        }

        public static void DisablePayment()
        {
            if (!IsOwner()) throw new Exception("No authorization.");
            AssetStorage.Disable();
        }

        private static bool IsOwner() => Runtime.CheckWitness(Owner);
    }
}
