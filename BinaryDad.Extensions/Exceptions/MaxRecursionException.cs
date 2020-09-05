using System;

namespace BinaryDad.Extensions
{
    [Serializable]
    public class MaxRecursionException : Exception
    {
        public MaxRecursionException() { }

        public MaxRecursionException(string message) : base(message) { }

        public MaxRecursionException(string message, Exception innerException) : base(message, innerException) { }
    }
}