using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitBreakerLib.Strategies
{
    class DelayStrategy : ICloseCircuitStrategy, IReopenCircuitStrategy
    {
        private int delayMiliseconds = 200;
        public void PlanForOpen(ICircuitBreaker circuitBreaker)
        {
            try
            {
                var task = Task.Delay(delayMiliseconds)
                               .ContinueWith(t => circuitBreaker.SwitchOn());
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        public bool CloseWhen(Exception ex) => ex != null;
    }
}
