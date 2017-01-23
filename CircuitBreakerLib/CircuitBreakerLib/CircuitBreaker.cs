using CircuitBreakerLib.Strategies;
using System;
using System.Threading;

namespace CircuitBreakerLib
{
    /// <summary>
    /// Capable to deceted failures of an external system and encapsulates logic to prevent a failure to reoccur.
    /// </summary>
    /// <remarks>
    /// The open/close operations are executed concurrently and are write-only. 
    /// The enter operation reads the circuit's state, but does not block.
    /// The implementations of the decision of opening the circuit and the strategy of closing back can be outside this class and hooked via the corresponding interfaces. This class also uses defaults for these interfaces. 
    /// </remarks>

    public sealed class CircuitBreaker : ICircuitBreaker, IDisposable
    {
        private ReaderWriterLockSlim _readerWriterLockSlim = new ReaderWriterLockSlim();
        private bool _closed = true;
        private Exception _systemException = null;

        private IOpenCircuitStrategy _openCircuitStrategy;
        private ICloseBackCircuitStrategy _closeBackCircuitStrategy;

        /// <summary>
        /// Creates a CircuitBreaker using default closing/reopening implementations. 
        /// </summary>
        public CircuitBreaker()
            : this(new OpenOnAnyFailureCircuit(), new DelayStrategy(200))
        {
        }

        /// <summary>
        /// Creates a CircuitBreaker based on the given strategies.
        /// </summary>
        /// <param name="openCircuitStrategy">Accepts an implementation for the decision of opening this circuit.</param>
        /// <param name="closeBackCircuitStrategy">Accepts an implementation for the when/how to close back this circuit.</param>
        public CircuitBreaker(IOpenCircuitStrategy openCircuitStrategy, ICloseBackCircuitStrategy closeBackCircuitStrategy)
        {
            if (openCircuitStrategy == null) throw new ArgumentNullException(nameof(openCircuitStrategy));
            if (closeBackCircuitStrategy == null) throw new ArgumentNullException(nameof(closeBackCircuitStrategy));

            _openCircuitStrategy = openCircuitStrategy;
            _closeBackCircuitStrategy = closeBackCircuitStrategy;
        }

        /// <summary>
        /// Executes the external system's action if the circuit breaker is close, otherwise throws the exception that made this circuit to open, so the external system is not accessed by other clients of this CircuitBreaker.
        /// The circuit opens if the original exception matches the closing strategy.
        /// A closing back operation is started after the circuit is closed.
        /// </summary>
        /// <param name="action">The delegate which encapsulates the access to the external system.</param>
        public void Enter(Action action)
        {
            TryEnterOtherwiseRethrow();

            //test the system
            try
            {
                action.Invoke();
            }
            catch (Exception exception)
            {
                SetSystemException(exception);

                ThrowSystemException();
            }
        }

        /// <summary>
        /// Let the current client enter the circuit, but if it is open, then throw.
        /// </summary>
        public void TryEnterOtherwiseRethrow()
        {
            //the circuit is open because of a previous failure of the external system
            //rethrow this failure back to the client as it would have been a new external system failure
            if (!_closed)
            {
                ThrowSystemException();
            }
        }

        /// <summary>
        /// Let the circuit to behave based on this exception.
        /// </summary>
        /// <param name="exception">The exception that might make the circuit to get opened.</param>
        public void SetSystemException(Exception exception)
        {
            _systemException = exception;

            if (_openCircuitStrategy.OpenWhen(exception))
            {
                Open();
                PlanCircuitForClosing();
            }
        }
        /// <summary>
        /// Close the circuit.
        /// </summary>
        public void CloseBack()
        {
            Close();
        }

        /// <summary>
        /// Close the circuit in a concurrent fashion. If another thread tries to close this circuit, then it has to wait first for this operation to finish.
        /// </summary>
        private void Close()
        {
            _readerWriterLockSlim.EnterWriteLock();
            _closed = true;
            _readerWriterLockSlim.ExitWriteLock();
        }

        /// <summary>
        /// Open the circuit in a concurrent fashion. If another thread tries to open this circuit, then it has to wait first for this operation to finish.
        private void Open()
        {
            _readerWriterLockSlim.EnterWriteLock();
            _closed = false;
            _readerWriterLockSlim.ExitWriteLock();
        }

        private void PlanCircuitForClosing()
        {
            _closeBackCircuitStrategy.Close(this);
        }

        private void ThrowSystemException()
        {
            throw _systemException;
        }

        /// <summary>
        /// Retruns if the CircuitBreaker is open or not
        /// </summary>
        public bool IsClosed => _closed;

        public CircuitBreakerScope GetScope()
        {
            return new CircuitBreakerScope(this);
        }
        /// <summary>
        /// Dispose the ReaderWriterLockSlim used by this class.
        /// </summary>
        public void Dispose()
        {
            if (_readerWriterLockSlim != null)
            {
                _readerWriterLockSlim.Dispose();
            }
        }
    }
}
