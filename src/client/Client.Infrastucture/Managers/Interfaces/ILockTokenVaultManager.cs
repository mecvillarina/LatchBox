using Client.Infrastructure.Models;
using Neo;
using Neo.Network.RPC.Models;
using Neo.Wallets;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace Client.Infrastructure.Managers.Interfaces
{
    public interface ILockTokenVaultManager : IManager
    {
        UInt160 ContractScriptHash { get; }
        Task<BigInteger> GetLatchBoxLocksLengthAsync();
        Task<UInt160> GetPaymentTokenScriptHashAsync();
        Task<BigInteger> GetPaymentTokenAddLockFeeAsync();
        Task<BigInteger> GetPaymentTokenClaimLockFee();
        Task<BigInteger> GetPaymentTokenRevokeLockFee();
        Task<LockTransaction> GetTransactionAsync(BigInteger lockIdx);
        Task<List<LockTransaction>> GetTransactionsByInitiatorAsync(string initiatorAddress);
        Task<List<LockTransaction>> GetTransactionsByReceiverAsync(string receiverAddress);
        Task<List<LockTransaction>> GetTransactionsByAssetAsync(UInt160 tokenScriptHash);
        Task<List<AssetRefund>> GetRefundsAsync(string accountAddress);
        Task<List<AssetCounter>> GetAssetsCounterAsync();
        Task<bool> ValidateNEP17TokenAsync(UInt160 tokenScriptHash);
        Task<RpcInvokeResult> ValidateAddLockAsync(UInt160 sender, UInt160 tokenAddress, BigInteger totalAmount, BigInteger durationInDays, List<LockReceiverArg> receiversArg, bool isRevocable);
        Task<RpcApplicationLog> AddLockAsync(KeyPair fromKey, UInt160 tokenAddress, BigInteger totalAmount, BigInteger durationInDays, List<LockReceiverArg> receiversArg, bool isRevocable);
        Task<RpcInvokeResult> ValidateClaimLockAsync(UInt160 account, BigInteger lockIndex);
        Task<RpcApplicationLog> ClaimLockAsync(KeyPair accountKey, BigInteger lockIndex);
        Task<RpcInvokeResult> ValidateRevokeLockAsync(UInt160 account, BigInteger lockIndex);
        Task<RpcApplicationLog> RevokeLockAsync(KeyPair accountKey, BigInteger lockIndex);
        Task<RpcInvokeResult> ValidateClaimRefundAsync(UInt160 account, UInt160 tokenAddress);
        Task<RpcApplicationLog> ClaimRefundAsync(KeyPair accountKey, UInt160 tokenAddress);
    }
}