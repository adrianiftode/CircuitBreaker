using System;
using System.Net.Mail;

namespace CircuitBreakerLib.Strategies
{
    public class SmtpCloseCircuit : ICloseCircuitStrategy
    {
        public bool CloseWhen(Exception exception) => exception is SmtpException;
    }
}
