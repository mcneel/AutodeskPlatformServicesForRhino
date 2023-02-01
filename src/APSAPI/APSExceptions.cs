using System;
using System.Runtime.Serialization;

namespace AutodeskPlatformServices
{
    public class APSException : Exception
    {
        public APSException() : base()
        {

        }

        public APSException(string message) : base(message)
        {
        }

        public APSException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected APSException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public class APINotAuthenticatedException : APSException
    {
        public APINotAuthenticatedException() : base("API is not authenticated") { }
    }
}
