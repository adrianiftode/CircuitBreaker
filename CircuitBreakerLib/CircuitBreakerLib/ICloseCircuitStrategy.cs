using System;

namespace CircuitBreakerLib
{
    /// <summary>
    /// Provides a strategy for when to close a circuit.
    /// </summary>
    public interface ICloseCircuitStrategy
    {
        /// <summary>
        /// Provide a boolean predicate about the external system failure to be used later by the circuit to decide if it should close or not.
        /// </summary>
        /// <param name="exception">The exception to inspect.</param>
        /// <returns>Returns true for when to close the circuit, otherwise false for not to close.</returns>
        bool CloseWhen(Exception exception);
    }
}
