using BlockScript.Reader;
using BlockScript.Utilities;
using FluentAssertions;
using Xunit;

namespace BlockScript.Tests.UtilitiesTests;

public class BufferTests
{
    [Fact]
    public void PeekNext_ShouldReturnNewItem_WhenBufferIsEmpty()
    {
        // Arrange
        var buffer = new Buffer<int>(() => 42);

        // Act
        var result = buffer.PeekNext();

        // Assert
        result.Should().Be(42);
    }

    [Fact]
    public void PeekNext_ShouldReturnSameItem_WhenCalledMultipleTimesWithoutTake()
    {
        // Arrange
        int counter = 0;
        var buffer = new Buffer<int>(() => ++counter);

        // Act
        var first = buffer.PeekNext();
        var second = buffer.PeekNext();
        buffer.Return();
        var third = buffer.PeekNext();

        // Assert
        first.Should().Be(1);
        second.Should().Be(2);
        third.Should().Be(1); // After Return(), we should get the first item again
    }

    [Fact]
    public void TakeAll_ShouldNotRemovedItems_WhenBufferIsEmpty()
    {
        // Arrange
        int counter = 0;
        var buffer = new Buffer<int>(() => ++counter);

        // Act
        buffer.TakeAll();
        var newItem = buffer.PeekNext(); // Should generate a fresh item

        // Assert
        newItem.Should().Be(1);
    }
    
    [Fact]
    public void TakeAll_ShouldRemoveUsedItems()
    {
        // Arrange
        int counter = 0;
        var buffer = new Buffer<int>(() => ++counter);

        // Act
        buffer.PeekNext(); // Generates 1
        buffer.PeekNext(); // Generates 2
        buffer.TakeAll();
        var newItem = buffer.PeekNext(); // Should generate a fresh item

        // Assert
        newItem.Should().Be(3);
    }
    
    [Fact]
    public void Take_ShouldRemoveOnlySpecifiedAmount()
    {
        // Arrange
        int counter = 0;
        var buffer = new Buffer<int>(() => ++counter);
        buffer.PeekNext(); // 1
        buffer.PeekNext(); // 2
        buffer.PeekNext(); // 3

        // Act
        buffer.Take(2);
        buffer.Return();
        var newItem = buffer.PeekNext();

        // Assert
        newItem.Should().Be(3); // Item 3 should still be in buffer
    }
    
    [Fact]
    public void Take_ShouldNotMovePointer()
    {
        // Arrange
        int counter = 0;
        var buffer = new Buffer<int>(() => ++counter);
        buffer.PeekNext(); // 1
        buffer.PeekNext(); // 2
        buffer.PeekNext(); // 3

        // Act
        buffer.Take(2);
        var newItem = buffer.PeekNext();

        // Assert
        newItem.Should().Be(4); // Item 3 should still be in buffer
    }
    
    
    [Fact]
    public void Take_WithPointerNotOnTheEnd_ShouldRemoveOnlySpecifiedAmount()
    {
        // Arrange
        int counter = 0;
        var buffer = new Buffer<int>(() => ++counter);
        buffer.PeekNext(); // 1
        buffer.PeekNext(); // 2
        buffer.PeekNext(); // 3

        // Act
        buffer.Return();
        buffer.Take(2);
        var newItem = buffer.PeekNext();

        // Assert
        newItem.Should().Be(3); // Item 3 should still be in buffer
    }

    [Fact]
    public void Take_ToManyElements_ShouldThrowException()
    {
        // Arrange
        var buffer = new Buffer<int>(() => 42);
        buffer.PeekNext();
        
        // Act
        var act = () => buffer.Take(2);

        // Assert
        act.Should().Throw<IndexOutOfRangeException>();
    }

    [Fact]
    public void Return_ShouldResetPointerWithoutClearingBuffer()
    {
        // Arrange
        int counter = 0;
        var buffer = new Buffer<int>(() => ++counter);

        // Act
        var first = buffer.PeekNext();
        var second = buffer.PeekNext();
        buffer.Return();
        var afterReturn = buffer.PeekNext();

        // Assert
        first.Should().Be(1);
        second.Should().Be(2);
        afterReturn.Should().Be(1); // Buffer should not be cleared, just reset
    }
}