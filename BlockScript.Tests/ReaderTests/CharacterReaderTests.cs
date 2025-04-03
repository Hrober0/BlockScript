using BlockScript.Reader;
using FluentAssertions;
using Xunit;

namespace BlockScript.Tests.ReaderTests;

public class CharacterReaderTests
{
        [Fact]
        public void GetCharacter_ShouldReturnEOT_ForEmptyInput()
        {
            // Arrange
            using TextReader streamReader = new StringReader("");
            var characterReader = new CharacterReader(streamReader);

            // Act
            var character = characterReader.GetCharacter();
            
            // Assert
            character.Char.Should().Be(CharacterReader.EndOfText);
            character.Line.Should().Be(1);
            character.Column.Should().Be(1);
        }
        
        [Fact]
        public void GetCharacter_ShouldReturnEOT_AfterEOT()
        {
            // Arrange
            using TextReader streamReader = new StringReader("");
            var characterReader = new CharacterReader(streamReader);
            
            // Act
            var character1 = characterReader.GetCharacter();
            var character2 = characterReader.GetCharacter();
            
            // Assert
            character1.Char.Should().Be(CharacterReader.EndOfText);
            character1.Line.Should().Be(1);
            character1.Column.Should().Be(1);
            
            character2.Char.Should().Be(CharacterReader.EndOfText);
            character2.Line.Should().Be(1);
            character2.Column.Should().Be(1);
        }
        
        [Fact]
        public void GetCharacter_ShouldReturnCharacter()
        {
            // Arrange
            using TextReader streamReader = new StringReader("+");
            var characterReader = new CharacterReader(streamReader);

            // Act
            var character1 = characterReader.GetCharacter();
            var character2 = characterReader.GetCharacter();
            
            // Assert
            character1.Char.Should().Be('+');
            character1.Line.Should().Be(1);
            character1.Column.Should().Be(1);
            
            character2.Char.Should().Be(CharacterReader.EndOfText);
            character2.Line.Should().Be(1);
            character2.Column.Should().Be(2);
        }
        
        [Theory]
        [InlineData(' ')]
        [InlineData('\t')]
        public void GetCharacter_ShouldUnifyWhiteSpace(char c)
        {
            // Arrange
            using TextReader streamReader = new StringReader(c.ToString());
            var characterReader = new CharacterReader(streamReader);

            // Act
            var character = characterReader.GetCharacter();
            
            // Assert
            character.Char.Should().Be(CharacterReader.WhiteSpace);
            character.Line.Should().Be(1);
            character.Column.Should().Be(1);
        }

        [Theory]
        [InlineData("\n")]   // Linux/macOS
        [InlineData("\r\n")] // Windows
        [InlineData("\r")]   // Old macOS
        public void GetCharacter_ShouldTrackLineAndColumn_WithDifferentNewlines(string newline)
        {
            // Arrange
            using TextReader streamReader = new StringReader("+" + newline + "-");
            var characterReader = new CharacterReader(streamReader);

            // Act
            var character1 = characterReader.GetCharacter();
            var character2 = characterReader.GetCharacter();
            var character3 = characterReader.GetCharacter();
            
            // Assert
            character1.Char.Should().Be('+');
            character1.Line.Should().Be(1);
            character1.Column.Should().Be(1);

            character2.Char.Should().Be(CharacterReader.NewLine);
            character2.Line.Should().Be(1);
            character2.Column.Should().Be(2);
            
            character3.Char.Should().Be('-');
            character3.Line.Should().Be(2);
            character3.Column.Should().Be(1);
        }
}