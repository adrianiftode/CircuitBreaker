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
        public void Swould_Throw_Exception_When_Action_Throws()
        {
            var circuitBreaker = new CircuitBreaker();

            Action act = () => circuitBreaker.PassThrough(() => { throw null; });

            act.ShouldThrowExactly<NullReferenceException>();
        }

        [Fact]
        public void Swould_NotThrow_Exception_When_Action_Doesnt_Throw()
        {
            var circuitBreaker = new CircuitBreaker();

            Action act = () => circuitBreaker.PassThrough(() => {; });

            act.ShouldNotThrow();
        }

        [Fact]
        public void Swould_Throw_The_Original_Exception_If_Accessed_After_IsClosed()
        {
            var circuitBreaker = new CircuitBreaker();

            Action act1 = () => circuitBreaker.PassThrough(() => { throw null; });
            Action act2 = () => circuitBreaker.PassThrough(() => { });

            act1.ShouldThrowExactly<NullReferenceException>();
            act2.ShouldThrowExactly<NullReferenceException>();
        }

        [Fact]
        public void Swould_Not_Throw_The_Original_Exception_If_Accessed_After_Is_Reopen()
        {
            var circuitBreaker = new CircuitBreaker();
            var reopenStrategyMock = new Mock<IReopenCircuitStrategy>();
            reopenStrategyMock
                .Setup(c => c.PlanForOpen(It.IsAny<ICircuitBreaker>()))
                .Callback(() => circuitBreaker.SwitchOn());
            circuitBreaker.WithReopenCircuitStrategy(reopenStrategyMock.Object);

            Action act1 = () => circuitBreaker.PassThrough(() => { throw null; });
            Action act2 = () => circuitBreaker.PassThrough(() => {; });

            act1.ShouldThrowExactly<NullReferenceException>();
            act2.ShouldNotThrow();
        }

        [Fact]
        public void Should_NotBeOpen_When_Action_Throws()
        {
            var circuitBreaker = new CircuitBreaker();

            try
            {
                circuitBreaker.PassThrough(() => { throw null; });
            }
            catch { }

            circuitBreaker.IsOpen.Should().BeFalse();
        }

        [Fact]
        public void Action_Should_Execute_When_Is_CircuitBreaker_Is_Open()
        {
            var circuitBreaker = new CircuitBreaker();
            bool actionState = false;
            Action action = () => actionState = true;

            circuitBreaker.PassThrough(action);

            actionState.Should().BeTrue();
        }

        [Fact]
        public void Action_Should_Not_Execute_When_Is_CircuitBreaker_Is_Closed()
        {
            var circuitBreaker = new CircuitBreaker();
            bool actionState = false;
            Action action = () => actionState = true;
            var reopenStrategyMock = new Mock<IReopenCircuitStrategy>();
            circuitBreaker.WithReopenCircuitStrategy(reopenStrategyMock.Object); //never reopen

            try
            {
                circuitBreaker.PassThrough(() => { throw null; });
                circuitBreaker.PassThrough(action);
            }
            catch { }

            actionState.Should().BeFalse();
        }
    }
}
