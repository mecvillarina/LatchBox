using Client.Infrastructure.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Neo;
using Neo.Network.RPC;
using System;
using System.IO;
using System.Net.Http;
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
    }
}
