using System;
namespace Celestron.NexStar
{
    public class NexStarReadException : Exception
    {
        public NexStarReadException(string message) : base(message) { }
    }
}
