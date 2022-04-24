using Client.Infrastructure.Extensions;
using Client.Infrastructure.Managers.Interfaces;
using Client.Infrastructure.Models;
using Client.Infrastructure.Models.Parameters;
using Neo;
using Neo.IO;
using Neo.Network.P2P.Payloads;
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
    public class VestingTokenVaultManager : ManagerBase, IVestingTokenVaultManager
    {
        public VestingTokenVaultManager(IManagerToolkit managerToolkit) : base(managerToolkit)
        {
        }

        public UInt160 ContractScriptHash => Neo.Network.RPC.Utility.GetScriptHash(ManagerToolkit.NeoSettings.VestingTokenVaultContractHash, ManagerToolkit.NeoProtocolSettings);

        public async Task<bool> ValidateNEP17TokenAsync(UInt160 tokenScriptHash)
        {
            var result = await ManagerToolkit.NeoContractClient.TestInvokeAsync(ContractScriptHash, "validateNEP17Token", tokenScriptHash).ConfigureAwait(false);
            return result.State == Neo.VM.VMState.HALT && result.Exception == null;
        }

        public async Task<UInt160> GetPaymentTokenScriptHashAsync()
        {
            var result = await ManagerToolkit.NeoContractClient.TestInvokeAsync(ContractScriptHash, "getPaymentTokenScriptHash").ConfigureAwait(false);
            return Neo.Network.RPC.Utility.GetScriptHash(result.Stack.Single().FromByteStringToAccount(), ManagerToolkit.NeoProtocolSettings);
        }

        public async Task<BigInteger> GetPaymentTokenAddVestingFeeAsync()
        {
            var result = await ManagerToolkit.NeoContractClient.TestInvokeAsync(ContractScriptHash, "getPaymentTokenAddVestingFee").ConfigureAwait(false);
            return result.Stack.Single().GetInteger();
        }

        public async Task<BigInteger> GetPaymentTokenClaimVestingFeeAsync()
        {
            var result = await ManagerToolkit.NeoContractClient.TestInvokeAsync(ContractScriptHash, "getPaymentTokenClaimVestingFee").ConfigureAwait(false);
            return result.Stack.Single().GetInteger();
        }

        public async Task<BigInteger> GetPaymentTokenRevokeVestingFeeAsync()
        {
            var result = await ManagerToolkit.NeoContractClient.TestInvokeAsync(ContractScriptHash, "getPaymentTokenRevokeVestingFee").ConfigureAwait(false);
            return result.Stack.Single().GetInteger();
        }

        public async Task<VestingTransaction> GetTransactionAsync(BigInteger vestingIdx)
        {
            var result = await ManagerToolkit.NeoContractClient.TestInvokeAsync(ContractScriptHash, "getVestingTransaction", vestingIdx).ConfigureAwait(false);
            var stack = result.Stack.First();
            return new VestingTransaction((Map)stack, ManagerToolkit.NeoProtocolSettings);
        }

        public async Task<List<VestingTransaction>> GetTransactionsByInitiatorAsync(string initiatorAddress)
        {
            List<VestingTransaction> transactions = new();

            var initiator = Neo.Network.RPC.Utility.GetScriptHash(initiatorAddress, ManagerToolkit.NeoProtocolSettings);
            var result = await ManagerToolkit.NeoContractClient.TestInvokeAsync(ContractScriptHash, "getVestingsByInitiator", initiator).ConfigureAwait(false);
            var stack = result.Stack.FirstOrDefault();

            if (stack != null)
            {
                var maps = (Neo.VM.Types.Array)stack;
                foreach (var map in maps)
                {
                    transactions.Add(new VestingTransaction((Map)map, ManagerToolkit.NeoProtocolSettings));
                }
            }

            return transactions;
        }

        public async Task<List<VestingTransaction>> GetTransactionsByReceiverAsync(string receiverAddress)
        {
            List<VestingTransaction> transactions = new();

            var receiver = Neo.Network.RPC.Utility.GetScriptHash(receiverAddress, ManagerToolkit.NeoProtocolSettings);
            var result = await ManagerToolkit.NeoContractClient.TestInvokeAsync(ContractScriptHash, "getVestingsByReceiver", receiver).ConfigureAwait(false);
            var stack = result.Stack.FirstOrDefault();

            if (stack != null)
            {
                var maps = (Neo.VM.Types.Map)stack;
                foreach (var map in maps)
                {
                    transactions.Add(new VestingTransaction((Map)map.Value, ManagerToolkit.NeoProtocolSettings));
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

        public async Task<RpcInvokeResult> ValidateAddVestingAsync(UInt160 sender, UInt160 tokenAddress, BigInteger totalAmount, bool isRevocable, List<VestingPeriodParameter> periods)
        {
            var periodArr = new Neo.VM.Types.Array();
            foreach (var period in periods)
            {
                var periodData = new Neo.VM.Types.Array();
                periodData.Add(period.Name);
                periodData.Add(period.TotalAmount);
                periodData.Add(period.DurationInDays);

                var receiverArr = new Neo.VM.Types.Array();
                foreach (var receiver in period.Receivers)
                {
                    var receiverData = new Neo.VM.Types.Array();
                    receiverData.Add(receiver.Name);
                    receiverData.Add(new ByteString(receiver.Address.ToArray()));
                    receiverData.Add(receiver.Amount);
                    receiverArr.Add(receiverData);
                }

                periodData.Add(receiverArr);

                periodArr.Add(periodData);
            }

            byte[] script = ContractScriptHash.MakeScript("addVesting", tokenAddress, totalAmount, isRevocable, periodArr.ToParameter());
            Signer[] signers = new[] { new Signer { Scopes = WitnessScope.Global, Account = sender } };

            return await ManagerToolkit.NeoRpcClient.InvokeScriptAsync(script, signers);
        }

        public async Task<RpcApplicationLog> AddVestingAsync(KeyPair fromKey, UInt160 tokenAddress, BigInteger totalAmount, bool isRevocable, List<VestingPeriodParameter> periods)
        {
            var periodArr = new Neo.VM.Types.Array();
            foreach (var period in periods)
            {
                var periodData = new Neo.VM.Types.Array();
                periodData.Add(period.Name);
                periodData.Add(period.TotalAmount);
                periodData.Add(period.DurationInDays);

                var receiverArr = new Neo.VM.Types.Array();
                foreach (var receiver in period.Receivers)
                {
                    var receiverData = new Neo.VM.Types.Array();
                    receiverData.Add(receiver.Name);
                    receiverData.Add(new ByteString(receiver.Address.ToArray()));
                    receiverData.Add(receiver.Amount);
                    receiverArr.Add(receiverData);
                }

                periodData.Add(receiverArr);

                periodArr.Add(periodData);
            }

            var sender = Contract.CreateSignatureRedeemScript(fromKey.PublicKey).ToScriptHash();
            
            byte[] script = ContractScriptHash.MakeScript("addVesting", tokenAddress, totalAmount, isRevocable, periodArr.ToParameter());
            Signer[] signers = new[] { new Signer { Scopes = WitnessScope.Global, Account = sender } };

            return await CreateAndExecuteTransactionAsync(script, signers, fromKey).ConfigureAwait(false);
        }

        public async Task<RpcInvokeResult> ValidateRevokeVestingAsync(UInt160 account, BigInteger vestingIndex)
        {
            byte[] script = ContractScriptHash.MakeScript("revokeVesting", vestingIndex);
            Signer[] signers = new[] { new Signer { Scopes = WitnessScope.Global, Account = account } };

            return await ManagerToolkit.NeoRpcClient.InvokeScriptAsync(script, signers);
        }

        public async Task<RpcApplicationLog> RevokeVestingAsync(KeyPair accountKey, BigInteger vestingIndex)
        {
            var sender = Contract.CreateSignatureRedeemScript(accountKey.PublicKey).ToScriptHash();

            byte[] script = ContractScriptHash.MakeScript("revokeVesting", vestingIndex);
            Signer[] signers = new[] { new Signer { Scopes = WitnessScope.Global, Account = sender } };

            return await CreateAndExecuteTransactionAsync(script, signers, accountKey).ConfigureAwait(false);
        }

        public async Task<RpcInvokeResult> ValidateClaimVestingAsync(UInt160 account, BigInteger vestingIndex, BigInteger periodIdx)
        {
            byte[] script = ContractScriptHash.MakeScript("claimVesting", vestingIndex, periodIdx);
            Signer[] signers = new[] { new Signer { Scopes = WitnessScope.Global, Account = account } };

            return await ManagerToolkit.NeoRpcClient.InvokeScriptAsync(script, signers);
        }

        public async Task<RpcApplicationLog> ClaimVestingAsync(KeyPair accountKey, BigInteger vestingIndex, BigInteger periodIdx)
        {
            var sender = Contract.CreateSignatureRedeemScript(accountKey.PublicKey).ToScriptHash();

            byte[] script = ContractScriptHash.MakeScript("claimVesting", vestingIndex, periodIdx);
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
