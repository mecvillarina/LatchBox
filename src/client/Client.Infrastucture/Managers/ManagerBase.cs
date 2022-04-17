using Client.Infrastructure.Managers.Interfaces;

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
