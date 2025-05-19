using BlockScript.Utilities;
using FluentAssertions;
using Xunit;

namespace BlockScript.Tests.UtilitiesTests
{
    public class StreamBufferTests
    {
        [Fact]
        public void Init_ShouldHasCorrectCurrentAndNext()
        {
            // Arrange
            int counter = 42;
            var buffer = new StreamBuffer<int>(() => counter++);

            // Assert
            buffer.Current.Should().Be(42);
            buffer.Next.Should().Be(43);
        }
        
        [Fact]
        public void Take_ShouldHasCorrectCurrentAndNext()
        {
            // Arrange
            int counter = 42;
            var buffer = new StreamBuffer<int>(() => counter++);

            // Act
            var result = buffer.Take();
            
            // Assert
            result.Should().Be(42);
            buffer.Current.Should().Be(43);
            buffer.Next.Should().Be(44);
        }
    }
}