using System;

namespace CircuitBreakerLib
{
    /// <summary>
    /// Capable to deceted failures of an external system and encapsulates logic to prevent a failure to reoccur.
    /// </summary>
    public interface ICircuitBreaker
    {
        /// <summary>
        /// The circuit should allow a client to use the external system and close it if the external system failed.
        /// </summary>
        /// <param name="action">Expresses the usage of the external system.</param>
        /// <returns>True if the the external system has been use with no failure, otherwise false.</returns>
        /// <exception cref="Action"></exception>
        bool TryUse(Action action);
        /// <summary>
        /// The implementation should provide a way to switch the breaker on in case of failure.
        /// </summary>
        void SwitchOn();
    }
}
