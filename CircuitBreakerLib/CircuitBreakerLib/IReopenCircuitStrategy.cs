
namespace CircuitBreakerLib
{
    /// <summary>
    /// Provides a strategy to close a circuit that has been open by an external system failure.
    /// </summary>
    public interface ICloseBackCircuitStrategy
    {
        /// <summary>
        /// Close the circuit using this strategy.
        /// </summary>
        /// <param name="circuitBreaker">The circuit breaker to close.</param>
        void Close(ICircuitBreaker circuitBreaker);
    }
}
