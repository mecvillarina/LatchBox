using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Client.Infrastructure.Exceptions
{
    [Serializable]
    public class ApiOkFailedException : Exception
    {
        public List<string> Messages { get; }

        public ApiOkFailedException(List<string> messages)
        {
            Messages = messages;
        }

        protected ApiOkFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
