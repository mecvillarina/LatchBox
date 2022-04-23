using Client.Infrastructure.Extensions;
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

        public UInt160 ContractScriptHash => Neo.Network.RPC.Utility.GetScriptHash(ManagerToolkit.NeoSettings.LockTokenVaultContractHash, ManagerToolkit.NeoProtocolSettings);

        public async Task<BigInteger> GetLatchBoxLocksLengthAsync()
        {
            var result = await ManagerToolkit.NeoContractClient.TestInvokeAsync(ContractScriptHash, "getLatchBoxLocksLength").ConfigureAwait(false);
            return result.Stack.Single().GetInteger();
        }

        public async Task<UInt160> GetPaymentTokenScriptHashAsync()
        {
            var result = await ManagerToolkit.NeoContractClient.TestInvokeAsync(ContractScriptHash, "getPaymentTokenScriptHash").ConfigureAwait(false);
            return Neo.Network.RPC.Utility.GetScriptHash(result.Stack.Single().FromByteStringToAccount(), ManagerToolkit.NeoProtocolSettings);
        }

        public async Task<BigInteger> GetPaymentTokenAddLockFeeAsync()
        {
            var result = await ManagerToolkit.NeoContractClient.TestInvokeAsync(ContractScriptHash, "getPaymentTokenAddLockFee").ConfigureAwait(false);
            return result.Stack.Single().GetInteger();
        }

        public async Task<BigInteger> GetPaymentTokenClaimLockFee()
        {
            var result = await ManagerToolkit.NeoContractClient.TestInvokeAsync(ContractScriptHash, "getPaymentTokenClaimLockFee").ConfigureAwait(false);
            return result.Stack.Single().GetInteger();
        }

        public async Task<BigInteger> GetPaymentTokenRevokeLockFee()
        {
            var result = await ManagerToolkit.NeoContractClient.TestInvokeAsync(ContractScriptHash, "getPaymentTokenRevokeLockFee").ConfigureAwait(false);
            return result.Stack.Single().GetInteger();
        }

        public async Task<LockTransaction> GetTransactionAsync(BigInteger lockIdx)
        {
            var result = await ManagerToolkit.NeoContractClient.TestInvokeAsync(ContractScriptHash, "getLatchBoxLockTransaction", lockIdx).ConfigureAwait(false);
            var stack = result.Stack.First();
            return new LockTransaction((Map)stack, ManagerToolkit.NeoProtocolSettings);
        }

        public async Task<List<LockTransaction>> GetTransactionsByInitiatorAsync(string initiatorAddress)
        {
            List<LockTransaction> transactions = new();

            var initiator = Neo.Network.RPC.Utility.GetScriptHash(initiatorAddress, ManagerToolkit.NeoProtocolSettings);
            var result = await ManagerToolkit.NeoContractClient.TestInvokeAsync(ContractScriptHash, "getLatchBoxLocksByInitiator", initiator).ConfigureAwait(false);
            var stack = result.Stack.FirstOrDefault();

            if (stack != null)
            {
                var maps = (Neo.VM.Types.Array)stack;
                foreach (var map in maps)
                {
                    transactions.Add(new LockTransaction((Map)map, ManagerToolkit.NeoProtocolSettings));
                }
            }

            return transactions;
        }

        public async Task<List<LockTransaction>> GetTransactionsByReceiverAsync(string receiverAddress)
        {
            List<LockTransaction> transactions = new();

            var receiver = Neo.Network.RPC.Utility.GetScriptHash(receiverAddress, ManagerToolkit.NeoProtocolSettings);
            var result = await ManagerToolkit.NeoContractClient.TestInvokeAsync(ContractScriptHash, "getLatchBoxLocksByReceiver", receiver).ConfigureAwait(false);
            var stack = result.Stack.FirstOrDefault();

            if (stack != null)
            {
                var maps = (Neo.VM.Types.Array)stack;
                foreach (var map in maps)
                {
                    transactions.Add(new LockTransaction((Map)map, ManagerToolkit.NeoProtocolSettings));
                }
            }

            return transactions;
        }

        public async Task<List<AssetRefund>> GetRefundsAsync(string accountAddress)
        {
            List<AssetRefund> refunds = new();

            var account = Neo.Network.RPC.Utility.GetScriptHash(accountAddress, ManagerToolkit.NeoProtocolSettings);

            byte[] script = ContractScriptHash.MakeScript("getRefunds");
            Signer[] signers = new[] { new Signer { Scopes = WitnessScope.Global, Account = account } };

            var result = await ManagerToolkit.NeoRpcClient.InvokeScriptAsync(script, signers);
            var stack = result.Stack.FirstOrDefault();

            if (stack != null)
            {
                var maps = (Map)stack;

                foreach (var map in maps)
                {
                    refunds.Add(new AssetRefund(map, ManagerToolkit.NeoProtocolSettings));
                }
            }
            return refunds;
        }

        public async Task<List<AssetCounter>> GetAssetsCounterAsync()
        {
            List<AssetCounter> assetsCounter = new();

            var result = await ManagerToolkit.NeoContractClient.TestInvokeAsync(ContractScriptHash, "getAssetsCounter").ConfigureAwait(false);
            var stack = result.Stack.FirstOrDefault();

            if (stack != null)
            {
                var maps = (Map)stack;
                foreach (var map in maps)
                {
                    assetsCounter.Add(new AssetCounter(map, ManagerToolkit.NeoProtocolSettings));
                }
            }

            return assetsCounter;
        }

        public async Task<bool> ValidateNEP17TokenAsync(UInt160 tokenScriptHash)
        {
            var result = await ManagerToolkit.NeoContractClient.TestInvokeAsync(ContractScriptHash, "validateNEP17Token", tokenScriptHash).ConfigureAwait(false);
            return result.State == Neo.VM.VMState.HALT && result.Exception == null;
        }

        public async Task<RpcInvokeResult> ValidateAddLockAsync(UInt160 sender, UInt160 tokenAddress, BigInteger totalAmount, BigInteger durationInDays, List<LockReceiverArg> receiversArg, bool isRevocable)
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

            return await ManagerToolkit.NeoRpcClient.InvokeScriptAsync(script, signers);
        }

        public async Task<RpcApplicationLog> AddLockAsync(KeyPair fromKey, UInt160 tokenAddress, BigInteger totalAmount, BigInteger durationInDays, List<LockReceiverArg> receiversArg, bool isRevocable)
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

        public async Task<RpcInvokeResult> ValidateRevokeLockAsync(UInt160 account, BigInteger lockIndex)
        {
            byte[] script = ContractScriptHash.MakeScript("revokeLock", lockIndex);
            Signer[] signers = new[] { new Signer { Scopes = WitnessScope.Global, Account = account } };

            return await ManagerToolkit.NeoRpcClient.InvokeScriptAsync(script, signers);
        }

        public async Task<RpcApplicationLog> RevokeLockAsync(KeyPair accountKey, BigInteger lockIndex)
        {
            var sender = Contract.CreateSignatureRedeemScript(accountKey.PublicKey).ToScriptHash();

            byte[] script = ContractScriptHash.MakeScript("revokeLock", lockIndex);
            Signer[] signers = new[] { new Signer { Scopes = WitnessScope.Global, Account = sender } };

            return await CreateAndExecuteTransactionAsync(script, signers, accountKey).ConfigureAwait(false);
        }

        public async Task<RpcInvokeResult> ValidateClaimLockAsync(UInt160 account, BigInteger lockIndex)
        {
            byte[] script = ContractScriptHash.MakeScript("claimLock", lockIndex);
            Signer[] signers = new[] { new Signer { Scopes = WitnessScope.Global, Account = account } };

            return await ManagerToolkit.NeoRpcClient.InvokeScriptAsync(script, signers);
        }

        public async Task<RpcApplicationLog> ClaimLockAsync(KeyPair accountKey, BigInteger lockIndex)
        {
            var sender = Contract.CreateSignatureRedeemScript(accountKey.PublicKey).ToScriptHash();

            byte[] script = ContractScriptHash.MakeScript("claimLock", lockIndex);
            Signer[] signers = new[] { new Signer { Scopes = WitnessScope.Global, Account = sender } };

            return await CreateAndExecuteTransactionAsync(script, signers, accountKey).ConfigureAwait(false);
        }

        public async Task<RpcInvokeResult> ValidateClaimRefundAsync(UInt160 account, UInt160 tokenAddress)
        {
            byte[] script = ContractScriptHash.MakeScript("claimRefund", tokenAddress);
            Signer[] signers = new[] { new Signer { Scopes = WitnessScope.Global, Account = account } };

            return await ManagerToolkit.NeoRpcClient.InvokeScriptAsync(script, signers);
        }

        public async Task<RpcApplicationLog> ClaimRefundAsync(KeyPair accountKey, UInt160 tokenAddress)
        {
            var account = Contract.CreateSignatureRedeemScript(accountKey.PublicKey).ToScriptHash();

            byte[] script = ContractScriptHash.MakeScript("claimRefund", tokenAddress);
            Signer[] signers = new[] { new Signer { Scopes = WitnessScope.Global, Account = account } };

            return await CreateAndExecuteTransactionAsync(script, signers, accountKey).ConfigureAwait(false);
        }
    }
}
