using Neo;
using Neo.VM;
using Neo.VM.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Infrastructure.Extensions
{
    public static class StackItemExtension
    {
        public static string FromByteStringToAccount(this StackItem item)
        {
            return (item.ToParameter().Value as byte[]).Reverse().ToArray().ToHexString();
        }
    }
}
