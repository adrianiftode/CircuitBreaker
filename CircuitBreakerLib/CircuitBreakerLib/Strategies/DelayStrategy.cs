using System;
using System.Threading.Tasks;

namespace CircuitBreakerLib.Strategies
{
    public class DelayStrategy : IReopenCircuitStrategy
    {
        private int _delayMiliseconds = 0;
        public DelayStrategy(int delayMiliseconds)
        {
            if (delayMiliseconds < 0) throw new ArgumentOutOfRangeException($"{nameof(delayMiliseconds)} should be a positive value.");
            this._delayMiliseconds = delayMiliseconds;
        }
        public void PlanForOpen(ICircuitBreaker circuitBreaker)
        {
            try
            {
                var task = Task.Delay(_delayMiliseconds)
                               .ContinueWith(t => circuitBreaker.SwitchOn());
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }
    }
}
