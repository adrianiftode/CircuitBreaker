using System;
using System.Net.Mail;

namespace CircuitBreakerLib.Strategies
{
    public class OpenOnSmtpFailureCircuit : IOpenCircuitStrategy
    {
        public bool OpenWhen(Exception exception) => exception is SmtpException;
    }
}