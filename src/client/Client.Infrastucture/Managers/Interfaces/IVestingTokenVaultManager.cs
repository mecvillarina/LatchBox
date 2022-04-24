using Client.Infrastructure.Models;
using Neo;
using Neo.Network.RPC.Models;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace Client.Infrastructure.Managers.Interfaces
{
    public interface IVestingTokenVaultManager : IManager
    {
        Task<RpcInvokeResult> ValidateAddVestingAsync(UInt160 sender, UInt160 tokenAddress, BigInteger totalAmount, bool isRevocable, List<VestingPeriodParameter> periods);
    }
}