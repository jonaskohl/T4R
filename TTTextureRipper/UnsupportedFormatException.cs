using System;
using System.Runtime.Serialization;

namespace TTTextureRipper
{
    [Serializable]
    internal class UnsupportedFormatException : Exception
    {
        public UnsupportedFormatException()
        {
        }

        public UnsupportedFormatException(string message) : base(message)
        {
        }

        public UnsupportedFormatException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UnsupportedFormatException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}