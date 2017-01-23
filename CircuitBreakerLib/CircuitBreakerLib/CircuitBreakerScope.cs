using System;
using System.Runtime.ExceptionServices;

namespace CircuitBreakerLib
{
    public class CircuitBreakerScope : ICircuitBreakerScope, IDisposable
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
