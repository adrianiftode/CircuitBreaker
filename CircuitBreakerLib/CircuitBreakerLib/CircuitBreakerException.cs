using System;

namespace CircuitBreakerLib
{
    public class CircuitBreakerException : Exception
    {
        private static readonly string GeneralExceptionMessage = 
            @"CircuitBreaker exception. This exception is raised when the circuit is closed an subsequent requests are sent. 
             It contains the original exception.";
        public CircuitBreakerException(Exception innerException) : base(GeneralExceptionMessage, innerException)
        {
        }
    }
}
