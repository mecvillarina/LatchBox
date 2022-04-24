using Client.Infrastructure.Models;
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
        Task<RpcInvokeResult> ValidateAddVestingAsync(UInt160 sender, UInt160 tokenAddress, BigInteger totalAmount, bool isRevocable, List<VestingPeriodParameter> periods);
        Task<RpcApplicationLog> AddVestingAsync(KeyPair fromKey, UInt160 tokenAddress, BigInteger totalAmount, bool isRevocable, List<VestingPeriodParameter> periods);
    }
}