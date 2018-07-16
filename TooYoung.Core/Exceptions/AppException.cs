using System;

namespace TooYoung.Core.Exceptions
{
    public class AppException : Exception
    {
        public int Code { get; set; }
        public AppException(string message) : base(message)
        {
        }

        public AppException(string message, int code) : base(message)
        {
            Code = code;
        }

        public AppException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
