using BlockScript.Interpreter;
using BlockScript.Interpreter.BuildInMethods;
using BlockScript.Lexer.FactorValues;
using BlockScript.Parser;
using FluentAssertions;
using Xunit;

namespace BlockScript.Tests.IntegrationTests;

public class End2EndTests
{
    private const string LIST = """
                                lNode = (lCurrent, lNext) => {
                                	(selector) => if selector {lCurrent} else {lNext};
                                };

                                lCurrent	= (list) => list(true);
                                lNext		= (list) => list(false);
                                isEmpty		= (list) => list == null;

                                getElement = (list, index) => {
                                    loop -isEmpty(list) && index > 0 {
                                        list := lNext(list);
                                        index := index - 1;
                                    };
                                    if index == 0 {
                                	    lCurrent(list)		# Found element
                                    }
                                };

                                setElement = (list, index, value) => {
                                    if -isEmpty(list) {
                                		if index == 0 {
                                			lNode(value, lNext(list));  									# Found element, so set its value
                                		}
                                		else {
                                		    newEnd = setElement(lNext(list), index - 1, value);
                                			lNode(lCurrent(list), newEnd);	# Recursive call for next element, and construct new node
                                		}
                                	}
                                };

                                getLength = (list) => {
                                	count = 0;
                                	loop -isEmpty(list) {
                                		list := lNext(list);
                                		count := count + 1;
                                	}
                                };
                                """;
    
    [Fact]
    public void Integration_ShouldCalculateFibonacci()
    {
        // Arrange
        // 1 2 3 5 8 13 21 34 55 
        // 1 2 3 4 5  6  7  8  9
        var input = """
                    fib = (i) => if i <= 1 1 else fib(i-1) + fib(i-2);
                    
                    debug(fib(1));
                    debug(fib(6));
                    debug(fib(9));
                    """;
        
        // Act
        var (returnValue, debug) = Execute(input);
        
        // Assert
        returnValue.Should().BeEquivalentTo(new IntFactor(55));
        debug.Should().BeEquivalentTo([
            new IntFactor(1),
            new IntFactor(13),
            new IntFactor(55),
        ]);
    }

    [Fact]
    public void Integration_ShouldPrintList()
    {
        // Arrange
        var input = LIST + """
                           list = lNode(3, null);
                           list = lNode(1, list);
                           list = lNode(4, list);
                           list = lNode(2, list);
                           list = lNode(5, list);

                           il = list;
                           loop -isEmpty(il) {
                               debug(lCurrent(il));
                               il := lNext(il);
                           };
                           """;
        
        // Act
        var (returnValue, debug) = Execute(input);
        
        // Assert
        returnValue.Should().BeOfType<NullFactor>();
        debug.Should().BeEquivalentTo([
            new IntFactor(5),
            new IntFactor(2),
            new IntFactor(4),
            new IntFactor(1),
            new IntFactor(3),
        ]);
    }
    
    [Fact]
    public void Integration_ShouldSetListElement()
    {
        // Arrange
        var input = LIST + """
                           list = lNode(3, null);
                           list = lNode(1, list);
                           list = lNode(4, list);
                           list = lNode(2, list);
                           list = lNode(5, list);
                           
                           list = setElement(list, 3, 9);
                           
                           il = list;
                           loop -isEmpty(il) {
                               debug(lCurrent(il));
                               il := lNext(il);
                           };
                           """;
        
        // Act
        var (returnValue, debug) = Execute(input);
        
        // Assert
        returnValue.Should().BeOfType<NullFactor>();
        debug.Should().BeEquivalentTo([
            new IntFactor(5),
            new IntFactor(2),
            new IntFactor(4),
            new IntFactor(9),
            new IntFactor(3),
        ]);
    }
    
    [Fact]
    public void Integration_ShouldReadListElement()
    {
        // Arrange
        var input = LIST + """
                           list = lNode(3, null);
                           list = lNode(1, list);
                           list = lNode(4, list);
                           list = lNode(2, list);
                           list = lNode(5, list);

                           length = getLength(list);
                           debug(length);
                           i = 0;
                           loop i < length {
                               debug(getElement(list, i));
                               i := i + 1;
                           };
                           """;
        
        // Act
        var (returnValue, debug) = Execute(input);
        
        // Assert
        returnValue.Should().Be(new IntFactor(5));
        debug.Should().BeEquivalentTo([
            new IntFactor(5),   // list size
            new IntFactor(5),
            new IntFactor(2),
            new IntFactor(4),
            new IntFactor(1),
            new IntFactor(3),
        ]);
    }
    
    [Fact]
    public void Integration_ShouldExecuteBubbleSort()
    {
        // Arrange
        var input = LIST + """
                        # bubble sort
                        bubbleSort = (list) => {
                            swapped = true;
                            length = getLength(list);
                            
                            loop swapped {
                                swapped := false;
                                i = 0;
                                loop i < length - 1 {
                                    if getElement(list, i) > getElement(list, i + 1) {
                                        temp = getElement(list, i);
                                        list := setElement(list, i, getElement(list, i + 1));
                                        list := setElement(list, i + 1, temp);
                                        swapped := true;
                                    };
                                    i := i + 1;
                                }
                            };
                            list
                        };

                        list = lNode(3, null);
                        list = lNode(1, list);
                        list = lNode(4, list);
                        list = lNode(2, list);
                        list = lNode(5, list);

                        list = bubbleSort(list);

                        il = list;
                        loop -isEmpty(il) {
                            debug(lCurrent(il));
                            il := lNext(il);
                        };

                        """;
        
        var (returnValue, debug) = Execute(input);
        
        // Assert
        returnValue.Should().BeOfType<NullFactor>();
        debug.Should().BeEquivalentTo([
            new IntFactor(1),
            new IntFactor(2),
            new IntFactor(3),
            new IntFactor(4),
            new IntFactor(5),
        ]);
    }
    
    [Fact]
    public void Integration_ShouldCallChainedMethods()
    {
        // Arrange
        var input = """
                    { (a) => { () => (b, c) => { debug(a); debug(b); debug(c) } } }(1)()(2,3);
                    """;
        
        // Act
        var (returnValue, debug) = Execute(input);
        
        // Assert
        returnValue.Should().BeEquivalentTo(new IntFactor(3));
        debug.Should().BeEquivalentTo([
            new IntFactor(1),
            new IntFactor(2),
            new IntFactor(3),
        ]);
    }
    
    private static (IFactorValue result, List<IFactorValue> debug) Execute(string input)
    {
        var debug = new List<IFactorValue>();
        using TextReader reader = new StringReader(input);
        var lexer = new Lexer.Lexer(reader);
        var parser = new LanguageParser(lexer.GetToken);
        var program = parser.ParserProgram();
        var interpreter = new LanguageInterpreter([new DebugMethod(debug)]);
        var returnValue = interpreter.ExecuteProgram(program);
        return (returnValue, debug);
    }
}