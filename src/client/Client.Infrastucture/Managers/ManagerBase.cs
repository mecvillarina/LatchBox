using Client.Infrastructure.Managers.Interfaces;
using Neo.Network.P2P.Payloads;
using Neo.Network.RPC;
using Neo.Network.RPC.Models;
using Neo.Wallets;
using System.Threading.Tasks;

namespace Client.Infrastructure.Managers
{
    public class ManagerBase
    {
        protected IManagerToolkit ManagerToolkit { get; }

        public ManagerBase(IManagerToolkit managerToolkit)
        {
            ManagerToolkit = managerToolkit;
        }

        public async Task<RpcInvokeResult> ExecuteTestInvokeAsync(byte[] script, Signer[] signers)
        {
            var result = await ManagerToolkit.NeoRpcClient.InvokeScriptAsync(script, signers);
            return result;
        }

        public async Task<RpcApplicationLog> CreateAndExecuteTransactionAsync(byte[] script, Signer[] signers, KeyPair fromKey)
        {
            var tx = await (await new TransactionManagerFactory(ManagerToolkit.NeoRpcClient).MakeTransactionAsync(script, signers).ConfigureAwait(continueOnCapturedContext: false)).AddSignature(fromKey).SignAsync().ConfigureAwait(continueOnCapturedContext: false);
            await ManagerToolkit.NeoRpcClient.SendRawTransactionAsync(tx).ConfigureAwait(continueOnCapturedContext: false);

            await ManagerToolkit.NeoWalletApi.WaitTransactionAsync(tx);
            var appLog = await ManagerToolkit.NeoRpcClient.GetApplicationLogAsync(tx.Hash.ToString());

            return appLog;
        }
    }
}
