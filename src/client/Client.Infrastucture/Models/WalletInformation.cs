using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Infrastructure.Models
{
    public class WalletInformation 
    {
        public string Filename { get; set; }
        public List<string> Addresses { get; set; }
    }
}
