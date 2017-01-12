
namespace CircuitBreakerLib
{
    /// <summary>
    /// Provides a strategy to reopen a closed circuit.
    /// </summary>
    public interface IReopenCircuitStrategy
    {
        /// <summary>
        /// Open the circuit by this strategy.
        /// </summary>
        /// <param name="circuitBreaker">The circuit breaker to open.</param>
        void PlanForOpen(ICircuitBreaker circuitBreaker);
    }
}
