using System;

namespace TooYoung.Core.Exceptions
{
    public class BlogAppException : Exception
    {
        public BlogAppException(string message) : base(message)
        {
        }

        public BlogAppException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
