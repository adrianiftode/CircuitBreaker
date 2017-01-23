using CircuitBreakerLib.Strategies;
using System;
using System.Threading;

namespace CircuitBreakerLib
{
    /// <summary>
    /// Remember the failure of an external system 
    /// and encapsulates logic to prevent further access to that.
    /// </summary>
    /// <remarks>
    /// The open/close operations are executed concurrently for write. 
    /// The enter operation reads the circuit's state, but does not block.
    /// 
    /// The implementations of the decision of opening the circuit 
    /// and the strategy of closing back can be outside this class 
    /// and hooked via the corresponding interfaces. 
    /// 
    /// This type uses defaults for these interfaces. 
    /// 
    /// By default the circuit will open on any failure 
    /// and it will delay the close back operation for 200 ms.
    /// </remarks>

    public sealed class CircuitBreaker : ICircuitBreaker, IDisposable
    {
        private ReaderWriterLockSlim _readerWriterLockSlim = new ReaderWriterLockSlim();
        private bool _closed = true;
        private Exception _systemException = null;

        private IOpenCircuitStrategy _openCircuitStrategy;
        private ICloseBackCircuitStrategy _closeBackCircuitStrategy;

        /// <summary>
        /// Creates a CircuitBreaker using the default opening/closing implementations. 
        /// 
        /// By default the circuit will open on any failure 
        /// and it will delay the close back operation for 200 ms.
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
        /// Executes the external system's action if the circuit breaker is closed.
        /// If the external system fails, then remember it's failure so
        /// further clients won't try to access again and perhaps wait
        /// and get the same failure.
        /// 
        /// As long as the circuit is open rethrow the external system's failure.
        /// Throws the original exception before this CircuitBreaker stores a reference to it.
        /// </summary>
        /// <param name="action">The delegate which encapsulates the access to the external system.</param>
        public void Enter(Action action)
        {
            TryEnterOtherwiseRethrow();

            //invoke the external system
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
        /// Let the current client enter the circuit, 
        /// but if it is not closed, 
        /// then throw the external's system failure that opened this circuit.
        /// </summary>
        public void TryEnterOtherwiseRethrow()
        {
            //the circuit is open because of a previous failure of the external system
            //rethrow this failure back to the client 
            //as it would have been a new external system failure
            if (!_closed)
            {
                ThrowSystemException();
            }
        }

        /// <summary>
        /// Set external's system failure based on which this circuit will behave.
        /// </summary>
        /// <param name="exception">The exception that might make the circuit to open.</param>
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
        /// Close the circuit in a concurrent fashion. 
        /// 
        /// If another thread tries to close this circuit, 
        /// then it has to wait first for this operation to finish.
        /// </summary>
        private void Close()
        {
            _readerWriterLockSlim.EnterWriteLock();
            _closed = true;
            _readerWriterLockSlim.ExitWriteLock();
        }

        /// <summary>
        /// Open the circuit in a concurrent fashion.
        /// 
        /// If another thread tries to open this circuit, 
        /// then it has to wait first for this operation to finish.
        private void Open()
        {
            _readerWriterLockSlim.EnterWriteLock();
            _closed = false;
            _readerWriterLockSlim.ExitWriteLock();
        }
        /// <summary>
        /// Invoke the close back strategy for this circuit.
        /// </summary>
        private void PlanCircuitForClosing()
        {
            _closeBackCircuitStrategy.Close(this);
        }
        /// <summary>
        /// Throw the external's system exception.
        /// </summary>
        private void ThrowSystemException()
        {
            if (_systemException != null)
            {
                throw _systemException;
            }
        }

        /// <summary>
        /// Retrun if this CircuitBreaker is closed or not.
        /// </summary>
        public bool IsClosed => _closed;
        /// <summary>
        /// Support for the .Net using statement 
        /// that can wrap a section of code that uses the external system.
        /// </summary>
        /// <returns>A disposable object.</returns>
        public IDisposable GetScope()
        {
            return new CircuitBreakerScope(this);
        }

        /// <summary>
        /// Dispose the resources used  by this class.
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
