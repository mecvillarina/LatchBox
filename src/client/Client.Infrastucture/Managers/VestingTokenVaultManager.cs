using Client.Infrastructure.Managers.Interfaces;
using Client.Infrastructure.Models;
using Neo;
using Neo.IO;
using Neo.Network.P2P.Payloads;
using Neo.Network.RPC.Models;
using Neo.VM;
using Neo.VM.Types;
using System.Collections.Generic;
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
    }
}
