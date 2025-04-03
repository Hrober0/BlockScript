using BlockScript.Lexer;
using FluentAssertions;
using Xunit;

namespace BlockScript.Tests.LexerTests
{
    public class LexerTests
    {
        [Fact]
        public void Lexer_ShouldReturnEOTToken_ForEmptyInput()
        {
            using TextReader reader = new StringReader("");
            var lexer = new Lexer.Lexer(reader);

            var token = lexer.GetToken();

            token.Should().NotBeNull();
            token.Type.Should().Be(TokenType.EOT);
            token.Line.Should().Be(1);
            token.Column.Should().Be(1);
        }
        
        [Fact]
        public void Lexer_ShouldReturnOperatorToken_ForOperators()
        {
            using TextReader reader = new StringReader("+");
            var lexer = new Lexer.Lexer(reader);

            var token = lexer.GetToken();

            token.Should().NotBeNull();
            token.Type.Should().Be(TokenType.Operator);
            token.Value.Should().Be("+");
            token.Line.Should().Be(1);
            token.Column.Should().Be(1);
        }

        [Theory]
        [InlineData("\n")]   // Linux/macOS
        [InlineData("\r\n")] // Windows
        [InlineData("\r")]   // Old macOS
        public void Lexer_ShouldTrackLineAndColumn_WithDifferentNewlines(string newline)
        {
            using TextReader reader = new StringReader("+" + newline + "-");
            var lexer = new Lexer.Lexer(reader);

            var firstToken = lexer.GetToken();
            var secondToken = lexer.GetToken();

            firstToken.Should().NotBeNull();
            firstToken.Type.Should().Be(TokenType.Operator);
            firstToken.Value.Should().Be("+");
            firstToken.Line.Should().Be(1);
            firstToken.Column.Should().Be(1);

            secondToken.Should().NotBeNull();
            secondToken.Type.Should().Be(TokenType.Operator);
            secondToken.Value.Should().Be("-");
            secondToken.Line.Should().Be(2);
            secondToken.Column.Should().Be(1);
        }
        
        [Fact]
        public void Lexer_ShouldSkipWhiteSpace()
        {
            using TextReader reader = new StringReader(" \t");
            var lexer = new Lexer.Lexer(reader);

            var token = lexer.GetToken();

            token.Should().NotBeNull();
            token.Type.Should().Be(TokenType.EOT);
            token.Line.Should().Be(1);
            token.Column.Should().Be(3);
        }
    }
}