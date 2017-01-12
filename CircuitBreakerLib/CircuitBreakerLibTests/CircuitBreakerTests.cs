using System;
using FluentAssertions;
using Moq;
using Xunit;
using CircuitBreakerLib;

namespace CircuitBreakerLibTests
{
    public class CircuitBreakerTests
    {
        [Fact]
        public void Ctor_Throws_ArgumentNullException_For_NullCloseCircuitStrategy()
        {
            var reopenCircuitStrategy = Mock.Of<IReopenCircuitStrategy>();

            Action act = () => new CircuitBreaker(null, reopenCircuitStrategy);

            act.ShouldThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public void Ctor_Throws_ArgumentNullException_For_Null_ReopenCircuitStrategy()
        {
            var closeCircuitStrategy = Mock.Of<ICloseCircuitStrategy>();

            Action act = () => new CircuitBreaker(closeCircuitStrategy, null);

            act.ShouldThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public void WithCloseCircuitStrategy_Throws_ArgumentNullException_For_NullArg()
        {
            var circuitBreaker = new CircuitBreaker();

            Action act = () => circuitBreaker.WithCloseCircuitStrategy(null);

            act.ShouldThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public void WithReopenCircuitStrategy_Throws_ArgumentNullException_For_NullArg()
        {
            var circuitBreaker = new CircuitBreaker();

            Action act = () => circuitBreaker.WithReopenCircuitStrategy(null);

            act.ShouldThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public void ShouldBeOpen_When_SwitchOn()
        {
            var circuitBreaker = new CircuitBreaker();

            circuitBreaker.SwitchOn();

            circuitBreaker.IsOpen.Should().BeTrue();
        }

        [Fact]
        public void Swould_Throw_CircuitBreakerException_When_Action_Throws()
        {
            var circuitBreaker = new CircuitBreaker();

            Action act = () => circuitBreaker.TryUse(() => { throw null; });

            act.ShouldThrowExactly<CircuitBreakerException>();
        }

        [Fact]
        public void CircuitBreakerException_Should_Contain_InnerException_When_Action_Throws()
        {
            var circuitBreaker = new CircuitBreaker();

            Action act = () => circuitBreaker.TryUse(() => { throw null; });

            act.ShouldThrowExactly<CircuitBreakerException>().And.InnerException.Should().BeOfType<NullReferenceException>();
        }

        [Fact]
        public void Should_NotBeOpen_When_Action_Throws()
        {
            var circuitBreaker = new CircuitBreaker();

            try
            {
                circuitBreaker.TryUse(() => { throw null; });
            }
            catch { }

            circuitBreaker.IsOpen.Should().BeFalse();
        }
    }
}
