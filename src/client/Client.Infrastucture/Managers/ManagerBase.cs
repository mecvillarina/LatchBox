using System.Net.Http;
using System.Threading.Tasks;

namespace Client.Infrastructure.Managers
{
    public class ManagerBase
    {
        protected IManagerToolkit ManagerToolkit { get; }
        protected string AccessToken { get; private set; }

        public ManagerBase(IManagerToolkit managerToolkit)
        {
            ManagerToolkit = managerToolkit;
        }
    }
}
