using System;
using FluentAssertions;
using Moq;
using Xunit;
using CircuitBreakerLib;

namespace CircuitBreakerLibTests
{
    public class CircuitBreakerScopeTests
    {
        [Fact]
        public void Ctor_Throws_ArgumentNullException_For_Null_CircuitBreaker()
        {
            Action act = () => new CircuitBreakerScope(null);

            act.ShouldThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public void Should_Not_Be_Closed_When_Scoped_Throws()
        {
            var circuitBreaker = new CircuitBreaker();

            try
            {
                using (var scope = circuitBreaker.GetScope())
                {
                    throw null;
                }
            }
            catch { }

            circuitBreaker.IsClosed.Should().BeFalse();
        }

        [Fact]
        public void Should_Not_Throw_Exception_When_Scoped_Doesnt_Throw()
        {
            var circuitBreaker = new CircuitBreaker();

            Action act = () =>
            {
                using (var scope = circuitBreaker.GetScope())
                {
                    ;
                }
            };

            act.ShouldNotThrow();
        }

        [Fact]
        public void Should_Be_Closed_When_Scoped_Doesnt_Throw()
        {
            var circuitBreaker = new CircuitBreaker();

            using (var scope = circuitBreaker.GetScope())
            {
                ;
            }

            circuitBreaker.IsClosed.Should().BeTrue();
        }

        [Fact]
        public void Should_Throw_The_Original_Exception_If_Accessed_When_Is_Open_When_Used_In_Scope()
        {
            var circuitBreaker = new CircuitBreaker();

            Action act1 = () =>
            {
                using (var scope = circuitBreaker.GetScope())
                {
                    throw null;
                }
            };
            Action act2 = () =>
            {
                using (var scope = circuitBreaker.GetScope())
                {
                    ;
                }
            };

            act1.ShouldThrowExactly<NullReferenceException>();
            act2.ShouldThrowExactly<NullReferenceException>();
        }

        [Fact]
        public void Action_Should_Not_Execute_In_Scoped_When_CircuitBreaker_Is_Not_Closed()
        {
            bool actionState = false;
            Action action = () => actionState = true;
            var circuitBreaker = new CircuitBreaker();

            //throw to open the circuit
            try
            {
                using (var scope = circuitBreaker.GetScope())
                {
                    throw null;
                }
            }
            catch { }
            try
            {
                using (var scope = circuitBreaker.GetScope())
                {
                    action();// should not be here
                }
            }
            catch { }

            actionState.Should().BeFalse();
        }

        [Fact]
        public void Should_Not_Throw_The_Original_Exception_If_Accessed_After_Is_ClosedBack_When_Used_In_Scope()
        {
            var reopenStrategyMock = new Mock<ICloseBackCircuitStrategy>();
            CircuitBreaker circuitBreaker = null;
            reopenStrategyMock
                .Setup(c => c.Close(It.IsAny<ICircuitBreaker>()))
                .Callback(() => circuitBreaker.CloseBack());
            circuitBreaker = new CircuitBreaker(Mock.Of<IOpenCircuitStrategy>(), reopenStrategyMock.Object);

            Action act1 = () =>
            {
                using (var scope = circuitBreaker.GetScope())
                {
                    throw null;
                }
            };
            Action act2 = () =>
            {
                using (var scope = circuitBreaker.GetScope())
                {
                    ;
                }
            };

            act1.ShouldThrowExactly<NullReferenceException>();
            act2.ShouldNotThrow();
        }

        [Fact]
        public void Should_Not_Be_Closed_When_OpenCircuitStrategy_Instructs_To_Open_When_Used_In_Scope()
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
                using (var scope = circuitBreaker.GetScope())
                {
                    throw null;
                }
            }
            catch { }

            circuitBreaker.IsClosed.Should().BeFalse();
        }
    }
}
