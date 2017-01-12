using System;
using System.Threading.Tasks;

namespace CircuitBreakerLib
{

    public class CircuitBreaker : ICircuitBreaker
    {
        private readonly object _mutex = new object();
        private bool open = true;
        private ICloseCircuitStrategy _closeCircuitStrategy;
        private IReopenCircuitStrategy _reopenCircuitStrategy;

        public CircuitBreaker()
        {
            _closeCircuitStrategy = new DelayStrategy();
            _reopenCircuitStrategy = new DelayStrategy();
        }

        public CircuitBreaker(ICloseCircuitStrategy closeCircuitStrategy, IReopenCircuitStrategy reopenCircuitStrategy)
        {
            if (closeCircuitStrategy == null) throw new ArgumentNullException(nameof(closeCircuitStrategy));
            if (reopenCircuitStrategy == null) throw new ArgumentNullException(nameof(reopenCircuitStrategy));

            _closeCircuitStrategy = closeCircuitStrategy;
            _reopenCircuitStrategy = reopenCircuitStrategy;
        }

        public void WithCloseCircuitStrategy(ICloseCircuitStrategy closeCircuitStrategy)
        {
            if (closeCircuitStrategy == null) throw new ArgumentNullException(nameof(closeCircuitStrategy));

            _closeCircuitStrategy = closeCircuitStrategy;
        }

        public void WithReopenCircuitStrategy(IReopenCircuitStrategy reopenCircuitStrategy)
        {
            if (reopenCircuitStrategy == null) throw new ArgumentNullException(nameof(reopenCircuitStrategy));

            _reopenCircuitStrategy = reopenCircuitStrategy;
        }
        public void SwitchOn()
        {
            Open();
        }
        private void Open()
        {
            lock (_mutex)
            {
                open = true;
            }
        }
        private void Close()
        {
            lock (_mutex)
            {
                open = false;
            }
        }

        public bool TryUse(Action action)
        {
            try
            {
                if (open)
                {
                    action();
                    return true;
                }
            }
            catch (Exception ex)
            {
                if (_closeCircuitStrategy.CloseWhen(ex))
                {
                    Close();
                    _reopenCircuitStrategy.PlanForOpen(this);
                }
                throw new CircuitBreakerException(ex);
            }
            return false;
        }

        public bool IsOpen => open;

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
}
