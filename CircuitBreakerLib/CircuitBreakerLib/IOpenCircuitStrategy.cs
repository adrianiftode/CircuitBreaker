using System;

namespace CircuitBreakerLib
{
    /// <summary>
    /// Provides a strategy for when to open a circuit. 
    /// An open circuit does not allow any other requests to the external system.
    /// </summary>
    public interface IOpenCircuitStrategy
    {
        /// <summary>
        /// Provide a boolean predicate about the external system failure 
        /// to be used later by the circuit to decide if it should open or not.
        /// </summary>
        /// <param name="exception">The exception to inspect.</param>
        /// <returns>Returns true for when to open the circuit, otherwise false for when not to open.</returns>
        bool OpenWhen(Exception exception);
    }
}
