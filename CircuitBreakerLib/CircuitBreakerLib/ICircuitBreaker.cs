using System;

namespace CircuitBreakerLib
{
    /// <summary>
    /// Capable to detect failures of an external system 
    /// and encapsulates logic to prevent further access to the external system.
    /// </summary>
    public interface ICircuitBreaker
    {
        /// <summary>
        /// Let the circuit to behave based on this exception.
        /// </summary>
        /// <param name="exception">The exception/failure that might make the circuit to open.</param>
        void SetSystemException(Exception exception);

        /// <summary>
        /// The circuit should allow a client to use the external system and open it if the external system failed.
        /// </summary>
        /// <param name="action">The usage of the external system.</param>
        /// <remarks>The implementation should rethrow the external system's failure for every new request, 
        /// even if the circuit is in the open state.</remarks>
        void Enter(Action action);
        /// <summary>
        /// Try to use the circuit breaker, but if it is open, then rethrow the external system's failure.
        /// </summary>
        void TryEnterOtherwiseRethrow();
        /// <summary>
        /// The implementation should provide a way to close the circuit breaker on demand.
        /// </summary>
        void CloseBack();
    }
}
