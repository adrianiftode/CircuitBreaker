using System;
using System.Runtime.ExceptionServices;

namespace CircuitBreakerLib
{
    /// <summary>
    /// Use the circuit to enter in its section on this object's creation 
    /// and set the failure, if any, at the Dispose call.
    /// 
    /// The access to the external system can be wraped with an `using` statement, 
    /// so we can use the fact that the Dispose method will always be called at the end of that code block.
    /// </summary>
    internal class CircuitBreakerScope : IDisposable
    {
        private ICircuitBreaker _circuit;

        public CircuitBreakerScope(ICircuitBreaker circuit)
        {
            if (circuit == null) throw new ArgumentNullException(nameof(circuit));

            _circuit = circuit;
            _circuit.TryEnterOtherwiseRethrow();

            AppDomain.CurrentDomain.FirstChanceException += FirstChanceExceptionEventHandler;
            
        }
        private void FirstChanceExceptionEventHandler(object sender, FirstChanceExceptionEventArgs args)
        {
            _circuit.SetSystemException(args.Exception);
        }
        public void Dispose()
        {
            AppDomain.CurrentDomain.FirstChanceException -= FirstChanceExceptionEventHandler;
            GC.SuppressFinalize(this);
        }
    }
}
