using Client.Infrastructure.Managers.Interfaces;
using Client.Infrastructure.Models;
using Neo;
using Neo.IO;
using Neo.Network.P2P.Payloads;
using Neo.Network.RPC;
using Neo.Network.RPC.Models;
using Neo.SmartContract;
using Neo.VM;
using Neo.VM.Types;
using Neo.Wallets;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Client.Infrastructure.Managers
{
    public class LockTokenVaultManager : ManagerBase, ILockTokenVaultManager
    {
        public LockTokenVaultManager(IManagerToolkit managerToolkit) : base(managerToolkit)
        {
        }

        public UInt160 ContractScriptHash => Neo.Network.RPC.Utility.GetScriptHash(ManagerToolkit.NeoSettings.LockTokenVaultContractHash, ProtocolSettings.Default);

        public async Task<BigInteger> GetLatchBoxLocksLength()
        {
            var result = await ManagerToolkit.NeoContractClient.TestInvokeAsync(ContractScriptHash, "getLatchBoxLocksLength").ConfigureAwait(false);
            return result.Stack.Single().GetInteger();
        }

        public async Task<bool> ValidateNEP17TokenAsync(UInt160 tokenScriptHash)
        {
            var result = await ManagerToolkit.NeoContractClient.TestInvokeAsync(ContractScriptHash, "validateNEP17Token", tokenScriptHash).ConfigureAwait(false);
            return result.State == Neo.VM.VMState.HALT && result.Exception == null;
        }

        public async Task<RpcInvokeResult> ValidateAddLockAsync(UInt160 sender, UInt160 tokenAddress, BigInteger totalAmount, BigInteger durationInDays, List<LatchBoxLockReceiverArg> receiversArg, bool isRevocable)
        {
            var receiverArr = new Neo.VM.Types.Array();
            foreach (var receiver in receiversArg)
            {
                var data = new Neo.VM.Types.Array();
                data.Add(new ByteString(receiver.ReceiverAddress.ToArray()));
                data.Add(receiver.Amount);
                receiverArr.Add(data);
            }

            byte[] script = ContractScriptHash.MakeScript("addLock", tokenAddress, totalAmount, durationInDays, receiverArr.ToParameter(), isRevocable);
            Signer[] signers = new[] { new Signer { Scopes = WitnessScope.Global, Account = sender } };

            return await ExecuteTestInvokeAsync(script, signers);
        }

        public async Task<RpcApplicationLog> AddLockAsync(KeyPair fromKey, UInt160 tokenAddress, BigInteger totalAmount, BigInteger durationInDays, List<LatchBoxLockReceiverArg> receiversArg, bool isRevocable)
        {
            var receiverArr = new Neo.VM.Types.Array();
            foreach (var receiver in receiversArg)
            {
                var data = new Neo.VM.Types.Array();
                data.Add(new ByteString(receiver.ReceiverAddress.ToArray()));
                data.Add(receiver.Amount);
                receiverArr.Add(data);
            }

            var sender = Contract.CreateSignatureRedeemScript(fromKey.PublicKey).ToScriptHash();

            byte[] script = ContractScriptHash.MakeScript("addLock", tokenAddress, totalAmount, durationInDays, receiverArr.ToParameter(), isRevocable);
            Signer[] signers = new[] { new Signer { Scopes = WitnessScope.Global, Account = sender } };

            return await CreateAndExecuteTransactionAsync(script, signers, fromKey).ConfigureAwait(false);
        }

        //var json = "{\"type\":\"Array\",\"value\":[{\"type\":\"Array\",\"value\":[{\"type\":\"ByteString\",\"value\":\"a0Or+iPNnPTNqV5b0v4IbIyA810=\"},{\"type\":\"Integer\",\"value\":1000000000}]}]}";
        //JObject jsonObj = JObject.Parse(json);
        //var s = Neo.Network.RPC.Utility.StackItemFromJson(jsonObj);

        //var base64 = Convert.ToBase64String(tokenAddress.ToArray());
        //var @struct = new Neo.VM.Types.Array();
        //@struct.Add(new ByteString(sender.ToArray()));
        //@struct.Add(totalAmount);
        //var array = new Neo.VM.Types.Array();
        //array.Add(@struct);

    }
}
