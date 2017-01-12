using CircuitBreakerLib.Strategies;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CircuitBreakerLib
{

    public class CircuitBreaker : ICircuitBreaker
    {
        private ReaderWriterLockSlim _readerWriterLockSlim = new ReaderWriterLockSlim();
        private bool _open = true;
        private Exception _systemException = null;

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

        public void PassThrough(Action action)
        {
            if (_open)
            {
                try
                {
                    action();
                }
                catch (Exception exception)
                {
                    _systemException = exception;
                    if (_closeCircuitStrategy.CloseWhen(exception))
                    {
                        Close();
                        _reopenCircuitStrategy.PlanForOpen(this);
                    }
                    throw exception;
                }
            }
            else
            {
                throw _systemException;
            }
        }

        public void SwitchOn()
        {
            Open();
        }
        private void Open()
        {
            _readerWriterLockSlim.EnterWriteLock();
            _open = true;
            _readerWriterLockSlim.ExitWriteLock();
        }
        private void Close()
        {
            _readerWriterLockSlim.EnterWriteLock();
            _open = false;
            _readerWriterLockSlim.ExitWriteLock();
        }

        public bool IsOpen => _open;

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
    }
}
