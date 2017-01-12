using System;

namespace CircuitBreakerLib.Strategies
{
    public class AlwaysCloseCircuit : ICloseCircuitStrategy
    {
        public bool CloseWhen(Exception exception) => exception != null;
    }
}
