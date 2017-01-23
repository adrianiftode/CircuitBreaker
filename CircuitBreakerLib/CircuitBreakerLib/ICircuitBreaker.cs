using System;

namespace CircuitBreakerLib
{
    /// <summary>
    /// Remeber the failure of an external system 
    /// and encapsulates logic to prevent further access to that.
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
        /// <remarks>The implementation should rethrow the external 
        /// system's failure for every new request, 
        /// even if the circuit is in the closed state.</remarks>
        void Enter(Action action);
        /// <summary>
        /// Try to use the circuit breaker, but if it is open, then rethrow the external system's failure.
        /// </summary>
        void TryEnterOtherwiseRethrow();
        /// <summary>
        /// Provide a way to close the circuit breaker on demand.
        /// </summary>
        void CloseBack();
        /// <summary>
        /// Support for the .Net using statement 
        /// that can wrap a section of code that uses the external system.
        /// </summary>
        /// <returns>A disposable object.</returns>
        IDisposable GetScope();
    }
}
