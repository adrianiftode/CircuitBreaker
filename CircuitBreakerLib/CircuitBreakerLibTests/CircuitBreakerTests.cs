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
        public void Ctor_Throws_ArgumentNullException_For_Null_CloseBackCircuitStrategy()
        {
            var closeBackCircuitStrategy = Mock.Of<ICloseBackCircuitStrategy>();

            Action act = () => new CircuitBreaker(null, closeBackCircuitStrategy);

            act.ShouldThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public void Ctor_Throws_ArgumentNullException_For_Null_ReopenCircuitStrategy()
        {
            var openCircuitStrategy = Mock.Of<IOpenCircuitStrategy>();

            Action act = () => new CircuitBreaker(openCircuitStrategy, null);

            act.ShouldThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public void Should_Not_Be_Closed_When_Action_Throws()
        {
            var circuitBreaker = new CircuitBreaker();

            try
            {
                circuitBreaker.Enter(() => { throw null; });
            }
            catch { }

            circuitBreaker.IsClosed.Should().BeFalse();
        }

        [Fact]
        public void Should_Be_Closed_When_CloseBack()
        {
            var circuitBreaker = new CircuitBreaker();

            circuitBreaker.CloseBack();

            circuitBreaker.IsClosed.Should().BeTrue();
        }

        [Fact]
        public void Should_Throw_Exception_When_Action_Throws()
        {
            var circuitBreaker = new CircuitBreaker();

            Action act = () => circuitBreaker.Enter(() => { throw null; });

            act.ShouldThrowExactly<NullReferenceException>();
        }

        [Fact]
        public void Should_Not_Throw_Exception_When_Action_Doesnt_Throw()
        {
            var circuitBreaker = new CircuitBreaker();

            Action act = () => circuitBreaker.Enter(() => {; });

            act.ShouldNotThrow();
        }

        [Fact]
        public void Should_Throw_The_Original_Exception_If_Accessed_When_Is_Open()
        {
            var circuitBreaker = new CircuitBreaker();

            Action act1 = () => circuitBreaker.Enter(() => { throw null; });
            Action act2 = () => circuitBreaker.Enter(() => { });

            act1.ShouldThrowExactly<NullReferenceException>();
            act2.ShouldThrowExactly<NullReferenceException>();
        }

        [Fact]
        public void Should_Not_Throw_The_Original_Exception_If_Accessed_After_Is_ClosedBack()
        {
            var reopenStrategyMock = new Mock<ICloseBackCircuitStrategy>();
            CircuitBreaker circuitBreaker = null;
            reopenStrategyMock
                .Setup(c => c.Close(It.IsAny<ICircuitBreaker>()))
                .Callback(() => circuitBreaker.CloseBack());
            circuitBreaker = new CircuitBreaker(Mock.Of<IOpenCircuitStrategy>(), reopenStrategyMock.Object);

            Action act1 = () => circuitBreaker.Enter(() => { throw null; });
            Action act2 = () => circuitBreaker.Enter(() => {; });

            act1.ShouldThrowExactly<NullReferenceException>();
            act2.ShouldNotThrow();
        }



        [Fact]
        public void Action_Should_Not_Execute_When_CircuitBreaker_Is_Not_Closed()
        {
            bool actionState = false;
            Action action = () => actionState = true;
            var circuitBreaker = new CircuitBreaker();

            try
            {
                circuitBreaker.Enter(() => { throw null; });
            }
            catch { }
            try
            {
                circuitBreaker.Enter(action);
            }
            catch { }

            actionState.Should().BeFalse();
        }

        [Fact]
        public void Should_Be_Closed_When_OpenCircuitStrategy_Does_Not_Instruct_To_Open()
        {
            var openCircuitStrategyMock = new Mock<IOpenCircuitStrategy>();
            openCircuitStrategyMock
                .Setup(c => c.OpenWhen(It.IsAny<Exception>()))
                .Returns(false);
            var circuitBreaker = new CircuitBreaker(
                    openCircuitStrategyMock.Object,
                    Mock.Of<ICloseBackCircuitStrategy>());

            try
            {
                circuitBreaker.Enter(() => { throw null; });
            }
            catch { }

            circuitBreaker.IsClosed.Should().BeTrue();
        }

        [Fact]
        public void Should_Not_Be_Closed_When_OpenCircuitStrategy_Instructs_To_Open()
        {
            var openCircuitStrategyMock = new Mock<IOpenCircuitStrategy>();
            openCircuitStrategyMock
                .Setup(c => c.OpenWhen(It.IsAny<Exception>()))
                .Returns(true);
            var circuitBreaker = new CircuitBreaker(
                    openCircuitStrategyMock.Object, 
                    Mock.Of<ICloseBackCircuitStrategy>());


            try
            {
                circuitBreaker.Enter(() => { throw null; });
            }
            catch { }

            circuitBreaker.IsClosed.Should().BeFalse();
        }



    }
}
