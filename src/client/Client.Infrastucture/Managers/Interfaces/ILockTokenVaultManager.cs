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
        Task<BigInteger> GetLatchBoxLocksLength();
        Task<UInt160> GetPaymentTokenScriptHashAsync();
        Task<BigInteger> GetPaymentTokenAddLockFeeAsync();
        Task<BigInteger> GetPaymentTokenClaimLockFee();
        Task<BigInteger> GetPaymentTokenRevokeLockFee();
        Task<LockTransaction> GetTransaction(BigInteger lockIdx);
        Task<List<LockTransaction>> GetTransactionsByInitiator(string initiatorAddress);
        Task<List<LockTransaction>> GetTransactionsByReceiver(string receiverAddress);
        Task<List<AssetRefund>> GetRefundsAsync(string accountAddress);
        Task<bool> ValidateNEP17TokenAsync(UInt160 tokenScriptHash);
        Task<RpcInvokeResult> ValidateAddLockAsync(UInt160 sender, UInt160 tokenAddress, BigInteger totalAmount, BigInteger durationInDays, List<LatchBoxLockReceiverArg> receiversArg, bool isRevocable);
        Task<RpcApplicationLog> AddLockAsync(KeyPair fromKey, UInt160 tokenAddress, BigInteger totalAmount, BigInteger durationInDays, List<LatchBoxLockReceiverArg> receiversArg, bool isRevocable);
        Task<RpcInvokeResult> ValidateClaimLockAsync(UInt160 account, BigInteger lockIndex);
        Task<RpcApplicationLog> ClaimLockAsync(KeyPair accountKey, BigInteger lockIndex);
        Task<RpcInvokeResult> ValidateRevokeLockAsync(UInt160 account, BigInteger lockIndex);
        Task<RpcApplicationLog> RevokeLockAsync(KeyPair accountKey, BigInteger lockIndex);
        Task<RpcInvokeResult> ValidateClaimRefundAsync(UInt160 account, UInt160 tokenAddress);
        Task<RpcApplicationLog> ClaimRefundAsync(KeyPair fromKey, UInt160 tokenAddress);
    }
}