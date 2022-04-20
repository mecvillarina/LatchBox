using Client.Infrastructure.Models;
using Neo;
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
        Task AddLock(KeyPair fromKey, UInt160 tokenAddress, BigInteger totalAmount, BigInteger durationInDays, List<LatchBoxLockReceiverArg> receivers, bool isRevocable);
    }
}