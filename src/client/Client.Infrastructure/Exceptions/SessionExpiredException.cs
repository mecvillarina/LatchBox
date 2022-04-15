using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Client.Infrastructure.Exceptions
{
    [Serializable]
    public class SessionExpiredException : Exception
    {
        public SessionExpiredException()
            : this("Session Expired")
        { }

        public SessionExpiredException(string message)
            : base(message)
        { }

        public SessionExpiredException(string message, Exception innerException)
            : base(message, innerException)
        { }

        protected SessionExpiredException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
