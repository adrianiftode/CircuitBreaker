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
    /// The pass through operation reads the circuit's state, but does not block.
    /// The implementations of the decision of closing the circuit and the strategy of reopening can be outside this class and hooked via the corresponding interfaces. This class uses also defaults for these interfaces. 
    /// </remarks>

    public class CircuitBreaker : ICircuitBreaker
    {
        private ReaderWriterLockSlim _readerWriterLockSlim = new ReaderWriterLockSlim();
        private bool _open = true;
        private Exception _systemException = null;

        private ICloseCircuitStrategy _closeCircuitStrategy;
        private IReopenCircuitStrategy _reopenCircuitStrategy;

        /// <summary>
        /// Creates a CircuitBreaker using default closing/reopening implementations. 
        /// </summary>
        public CircuitBreaker()
            : this(new AlwaysCloseCircuit(), new DelayStrategy(200))
        {
        }

        /// <summary>
        /// Creates a CircuitBreaker based on the given close circuit strategy and reopen circuit strategy.
        /// </summary>
        /// <param name="closeCircuitStrategy">Accepts an implementation for the decision of closing this circuit.</param>
        /// <param name="reopenCircuitStrategy">Accepts an implementation for the reopening this circuit.</param>
        public CircuitBreaker(ICloseCircuitStrategy closeCircuitStrategy, IReopenCircuitStrategy reopenCircuitStrategy)
        {
            if (closeCircuitStrategy == null) throw new ArgumentNullException(nameof(closeCircuitStrategy));
            if (reopenCircuitStrategy == null) throw new ArgumentNullException(nameof(reopenCircuitStrategy));

            _closeCircuitStrategy = closeCircuitStrategy;
            _reopenCircuitStrategy = reopenCircuitStrategy;
        }

        /// <summary>
        /// Executes the external system's action if the circuit breaker is open, otherwise throws the exception that closed this circuit so the external system is not accessed by other clients of this CircuitBreaker.
        /// The circuit is closed if the original exception matches the closing strategy.
        /// A reopening operation is started if the circuit get's closed.
        /// </summary>
        /// <param name="action">The delegate which encapsulates the access to the external system.</param>
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
        /// <summary>
        /// Reopens the circuit.
        /// </summary>
        public void SwitchOn()
        {
            Open();
        }

        /// <summary>
        /// Opens the circuit in a concurrent fashion. If another thread tries to close this circuit, then it has to wait first for this operation to finish.
        /// </summary>
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

        /// <summary>
        /// Retruns if the CircuitBreaker is open or not
        /// </summary>
        public bool IsOpen => _open;

        /// <summary>
        /// Replace the circuit strategy. This operation creates a new CircuitBreaker using the other dependecies, but does not carry the state this one.
        /// </summary>
        /// <param name="closeCircuitStrategy">Accepts an implementation for the decision of closing this circuit.</param>
        /// <param name="reopenCircuitStrategy">Accepts an implementation for the reopening this circuit.</param>
        /// <returns>A new CircuitBreaker</returns>
        public CircuitBreaker WithCloseCircuitStrategy(ICloseCircuitStrategy closeCircuitStrategy)
        {
            if (closeCircuitStrategy == null) throw new ArgumentNullException(nameof(closeCircuitStrategy));

            return new CircuitBreaker(closeCircuitStrategy, this._reopenCircuitStrategy);
        }

        /// <summary>
        /// Replace the circuit strategy. This operation creates a new CircuitBreaker using the other dependecies, but does not carry the state this one.
        /// </summary>
        /// <param name="reopenCircuitStrategy">Accepts an implementation for the reopening this circuit.</param>
        /// <returns>A new CircuitBreaker</returns>
        public CircuitBreaker WithReopenCircuitStrategy(IReopenCircuitStrategy reopenCircuitStrategy)
        {
            if (reopenCircuitStrategy == null) throw new ArgumentNullException(nameof(reopenCircuitStrategy));

            return new CircuitBreaker(this._closeCircuitStrategy, reopenCircuitStrategy);
        }
    }
}
