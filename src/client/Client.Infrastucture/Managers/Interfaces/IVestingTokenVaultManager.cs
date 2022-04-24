using Client.Infrastructure.Models;
using Client.Infrastructure.Models.Parameters;
using Neo;
using Neo.Network.RPC.Models;
using Neo.Wallets;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace Client.Infrastructure.Managers.Interfaces
{
    public interface IVestingTokenVaultManager : IManager
    {
        UInt160 ContractScriptHash { get; }
        Task<bool> ValidateNEP17TokenAsync(UInt160 tokenScriptHash);
        Task<UInt160> GetPaymentTokenScriptHashAsync();
        Task<BigInteger> GetPaymentTokenAddVestingFeeAsync();
        Task<BigInteger> GetPaymentTokenClaimVestingFee();
        Task<BigInteger> GetPaymentTokenRevokeVestingFee();
        Task<VestingTransaction> GetTransactionAsync(BigInteger vestingIdx);
        Task<List<VestingTransaction>> GetTransactionsByInitiatorAsync(string initiatorAddress);
        Task<List<VestingTransaction>> GetTransactionsByReceiverAsync(string receiverAddress);
        Task<List<AssetRefund>> GetRefundsAsync(string accountAddress);
        Task<RpcInvokeResult> ValidateAddVestingAsync(UInt160 sender, UInt160 tokenAddress, BigInteger totalAmount, bool isRevocable, List<VestingPeriodParameter> periods);
        Task<RpcApplicationLog> AddVestingAsync(KeyPair fromKey, UInt160 tokenAddress, BigInteger totalAmount, bool isRevocable, List<VestingPeriodParameter> periods);
        Task<RpcInvokeResult> ValidateRevokeVestingAsync(UInt160 account, BigInteger vestingIndex);
        Task<RpcApplicationLog> RevokeVestingAsync(KeyPair accountKey, BigInteger vestingIndex);
        Task<RpcInvokeResult> ValidateClaimVestingAsync(UInt160 account, BigInteger vestingIndex, BigInteger periodIdx);
        Task<RpcApplicationLog> ClaimVestingAsync(KeyPair accountKey, BigInteger vestingIndex, BigInteger periodIdx);
        Task<RpcInvokeResult> ValidateClaimRefundAsync(UInt160 account, UInt160 tokenAddress);
        Task<RpcApplicationLog> ClaimRefundAsync(KeyPair accountKey, UInt160 tokenAddress);
    }
}