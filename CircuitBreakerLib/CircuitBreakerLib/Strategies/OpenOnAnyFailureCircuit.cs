using System;

namespace CircuitBreakerLib.Strategies
{
    public class OpenOnAnyFailureCircuit : IOpenCircuitStrategy
    {
        public bool OpenWhen(Exception exception) => exception != null;
    }
}
