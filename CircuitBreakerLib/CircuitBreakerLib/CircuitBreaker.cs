using CircuitBreakerLib.Strategies;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CircuitBreakerLib
{

    public class CircuitBreaker : ICircuitBreaker
    {
        private readonly object _mutex = new object();
        private bool open = true;
        private ICloseCircuitStrategy _closeCircuitStrategy;
        private IReopenCircuitStrategy _reopenCircuitStrategy;
        private Exception exception;
        private ReaderWriterLockSlim readerWriterLockSlim = new ReaderWriterLockSlim();

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
            readerWriterLockSlim.EnterWriteLock();
            open = true;
            readerWriterLockSlim.ExitWriteLock();
        }
        private void Close()
        {
            readerWriterLockSlim.EnterWriteLock();
            open = false;
            readerWriterLockSlim.ExitWriteLock();
        }

        public void PassThrough(Action action)
        {
            if (open)
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    exception = ex;
                    if (_closeCircuitStrategy.CloseWhen(ex))
                    {
                        Close();
                        _reopenCircuitStrategy.PlanForOpen(this);
                    }
                    throw ex;
                }
            }
            else
            {
                throw exception;
            }
        }

        public bool IsOpen => open;
    }
}
