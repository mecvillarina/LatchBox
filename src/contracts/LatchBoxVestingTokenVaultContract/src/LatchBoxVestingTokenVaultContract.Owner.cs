using System.ComponentModel;

using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;

namespace LatchBoxVestingTokenVaultContract
{
    public partial class LatchBoxVestingTokenVaultContract : SmartContract
    {
        private const byte Prefix_OwnerAddress = 0xFF;

        private static UInt160 GetOwnerAddress()
        {
            var storageKey = new byte[] { Prefix_OwnerAddress };
            return (UInt160)Storage.Get(Storage.CurrentContext, storageKey);
        }

        private static bool IsOwner()
        {
            var contractOwner = GetOwnerAddress();
            return contractOwner.Equals(Tx.Sender) && Runtime.CheckWitness(contractOwner);
        }

        [DisplayName("_deploy")]
        public static void Deploy(object data, bool update)
        {
            if (update) return;

            Storage.Put(Storage.CurrentContext, new byte[] { Prefix_OwnerAddress }, Tx.Sender);
        }

        public static void Update(ByteString nefFile, string manifest)
        {
            if (!IsOwner()) ReportErrorAndThrow("No authorization.");
            ContractManagement.Update(nefFile, manifest, null);
        }

        public static void Destroy()
        {
            if (!IsOwner()) ReportErrorAndThrow("No authorization.");
            ContractManagement.Destroy();
        }
    }
}