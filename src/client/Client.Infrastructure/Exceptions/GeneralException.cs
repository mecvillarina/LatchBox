using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Client.Infrastructure.Exceptions
{
    [Serializable]
    public class GeneralException : Exception
    {
        public GeneralException(string message, string messageTitle) : base(message)
        {
            MessageTitle = messageTitle;
        }

        public GeneralException(string message, string messageTitle, Exception innerException) : base(message, innerException)
        {
            MessageTitle = messageTitle;
        }

        public GeneralException()
        {
        }

        public GeneralException(string message) : base(message)
        {
        }

        public GeneralException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected GeneralException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string MessageTitle { get; }
    }
}
