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
        Task<LockTransaction> GetTransaction(BigInteger lockIdx);
        Task<List<LockTransaction>> GetTransactionsByInitiator(string initiatorAddress);
        Task<List<LockTransaction>> GetTransactionsByReceiver(string receiverAddress);
        Task<List<AssetRefund>> GetRefundsAsync(string accountAddress);
        Task<bool> ValidateNEP17TokenAsync(UInt160 tokenScriptHash);
        Task<RpcInvokeResult> ValidateAddLockAsync(UInt160 sender, UInt160 tokenAddress, BigInteger totalAmount, BigInteger durationInDays, List<LatchBoxLockReceiverArg> receiversArg, bool isRevocable);
        Task<RpcApplicationLog> AddLockAsync(KeyPair fromKey, UInt160 tokenAddress, BigInteger totalAmount, BigInteger durationInDays, List<LatchBoxLockReceiverArg> receiversArg, bool isRevocable);
        Task<RpcInvokeResult> ValidateRevokeLockAsync(UInt160 sender, BigInteger lockIndex);
        Task<RpcApplicationLog> RevokeLockAsync(KeyPair fromKey, BigInteger lockIndex);
    }
}