using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Infrastructure.Models
{
    public class NeoProtocolConfiguration
    {
        public long Network { get; set; }
        public long AddressVersion { get; set; }
        public long MillisecondsPerBlock { get; set; }
        public long MaxTransactionsPerBlock { get; set; }
        public long MemoryPoolMaxTransactions { get; set; }
        public long MaxTraceableBlocks { get; set; }
        public long InitialGasDistribution { get; set; }
        public long ValidatorsCount { get; set; }
        public List<string> StandbyCommittee { get; set; }
        public List<string> SeedList { get; set; }
    }

}
