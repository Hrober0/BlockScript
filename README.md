# BlockScript

## Introduction
BlockScript is a general-purpose language focused on advanced function handling.
It is dynamically typed with mutable variables by default and parameters passed by copy.
It supports higher-order functions, function nesting, and passing functions as parameters.

The language emphasizes the broad use of blocks, i.e., chunks of code that can be used as conditional values or parameters.

## Language principles

### Data types
- `int`
- `bool`
- `string`
- `null`

### Arithmetic and Logical Operators

*(Ordered by decreasing priority)*

* `*`  – multiplication
* `/`  – division
* `+`  – addition
* `-`  – subtraction or negation
* `??` – non-null value
* `>`  – greater than
* `>=` – greater than or equal to
* `<`  – less than
* `<=` – less than or equal to
* `==` – equals
* `!=` – not equal
* `&&` – and
* `||` – or

### Supported Data Types for Operators

* bool: `||`, `&&`, `-`
* int: `+`, `-`, `*`, `/`, `>`, `>=`, `<`, `<=`
* string: `+`
* bool, int, string, null: `??`, `==`, `!=`

*(Other types will result in an error)*

### Assignment Operators

* `=`  – declaration
* `?=` – conditional assignment
* `:=` – assignment

## Variables

* A variable has a type, but it can change over time.
* Operations on variables of different types do not require explicit casting.

**Examples**:  

```py
a = 4;
a = "a";		# a changes the type to string

b = null;
b ?= 3;			# b assign to 3, because it was null

b ?= 4;			# b ramians 3, because it was not null

b = a ?? 5;		# b assign to 4

a = null;
b = a ?? 5;		# b assign to 5

c = 2;
f = () => c = 3;
print(c);       	# prints 2
f();
print(c);       	# prints 2, because new c was declared in local context

d = 2;
f = () => d := 3;
print(d);       	# prints 2
f();
print(d);       	# prints 3, because c value was overided

e := 2;         	# error e was not defined
e ?= 2;        	 	# error e was not defined
e = 2;         		# works
```

**Type conversion**:

```py
# int + string => str(int) + string
3 + "a" => "3a"
"a" + 3 => "a3"

# bool || string 	=> bool || bool(string)
false || ""		=> false
false || "e"		=> true
true && ""		=> false
"" && true 		=> false
true && "a"		=> true

# bool || int		=> bool || bool(int)
false || 0		=> false
false || 1		=> true
true && -1		=> false
true && 2		=> true
2 && true		=> true
```

### Comments
\# this is a comment

### Block
An important aspect of the language to understand is that a **block** consists of consecutive lines of code.

- The entire program is a block.  
- A block has its own **context**, meaning its own local memory, which contains:  
  - all declared variables (including functions).  
  - a reference to the **context** of the parent block.  
- Blocks can be nested.  
- Each block returns a value equal to the value of the last instruction in the block.  
- The `;` symbol is used to separate instructions from one another; it may or may not appear at the end of a block.
- Code inside `{}` always makes a block, it is important in consideration of statements that using blocks, like `if` or `loop`
    - `if a == 2 print(1)` <- there is not blocks
    - `if { a == 2 } { print(1) }` <- there is two block
    - `loop { a = a + 1; a < 2 } print(a)` <- there is one block

**Examples**:

```py
{
    a=3;
    b=4;
}
# blok returns: 4

{
    a=3;
    b={4};	# the same as b=4;
    a+b;
}
# blok returns: 7

{
    a=3;
    b=4;
    a>b		# last statement doesn't reqire ;
}
# blok returns: false

{
    a=3;
    b=4;
    ()=>{a>b};
}
# blok returns: function call ()=>{a>b}

{
    print("a");
}
# blok returns: "a"

{
	a = 4;
	c = {
		a := a - 1;	# assign existing value
		b = a - 1;
	};
	print(a);
	print(c - 1);
}
# blok returns: 1
# varaible b is not accesibile form outside blok
# prints: 3 1

{
    print(1);
    break; 
    print(2);
}
# blok returns: 1
# prints: 1
# break exits from block

{
    print(1);
    {
        print(2);    
        break;
        print(3); 
    }
    print(4);
}
# blok returns: 4
# prints: 1 2 4

{
    print(1);
    {
        print(2);    
        break 2;
        print(3); 
    }
    print(4);
}
# blok returns: 2
# prints: 1 2
# break can accept one argument, that indicate with how many block break will exist
# break without argument is equiwalent of `break 1`

{
    print(1);
    break 0; 
}
# blok returns: 1
# prints: 1
# break with not positive value is ignored, as well as its value
```

### Conditional Statements
Conditional statements resemble the ternary operator.

**Examples**:
```py
if {a>3} {print(a)};
# when a > 3, it prints a and returns a; otherwise, it returns null;

if a>3 {print(a)};
# a single expression can follow `if`; when a > 3, it prints a and returns a; otherwise, it returns null;

if a>3 print(a);
# a single expression can follow the condition;

if a>3 {print(a)} else {"no"};
# when a > 3, it prints a and returns a; otherwise, it returns "no";

if a>3||a<2 {print(a); a+1};
# when a > 3 or a < 2, it prints a and returns a+1; otherwise, it returns null;

2 + 2 * 2 > 7 || {if 3>2 {1}};
# {2 + 4 > 7 || 1}
# {6 > 7 || 1}
# {false || true}
# true

# true || false && true
# returns false, because && has higher precedence

a = 1;
if 2 > a print("ok") else print("no");
# prints ok

a = 1;
print(if 2 > a "ok" else "no");
# same as above, written differently

a = 3;
print(if 2 > a "ok");
# prints null

a = "a";
print(if a "ok" else "no");
# prints ok – string parsed to boolean gives true

a = "";
print(if a "ok" else "no");
# prints no – empty string parsed to boolean gives false

a = 3;
print(if a "ok" else "no");
# prints ok – integer parsed to boolean gives true

a = 0;
print(if a "ok" else "no");
# prints no – zero parsed to boolean gives false

if a==1 1
else if a==2 2
else "no";
# if a==1, returns 1; if a==2, returns 2; otherwise, returns "no"
```

### Functions

**Examples**:
```py
f=()=>{print("a")};
f();
# prints: "a"


f=()=>print("a");
print("b");
f();
# prints: "b" "a"


f=(a)=>print(a);
f("b");
# prints: "b"


f = () => {
	a = 2;
	ff = () => { a };
	print(ff());
	a = 3;
	print(ff());
};
f();
# prints: 2 3
# function "ff" has access to the parent context "f", so it uses the current value of "a"


f = () => {
	a = 2;
	ff = () => { a := a + 1 };
	a = 3;
	ff;
};
fc = f();
print(fc());	# prints 4
fc = f();
print(fc());	# prints 4
print(fc());	# prints 5
print(fc());	# prints 6
# function "ff" has access to the context of the parent function "f", even after "f" has finished executing
# therefore "ff" uses the last value of "a", which is 3, and increments it
# "print(f()())" creates a new context for "f", so each call resets to 3 and returns 4
# assigning "fc = f();" stores a function with a reference to the context of "f",
# so calling "fc()" repeatedly modifies the same context, resulting in incrementing values


f = (n) => {
	if n <= 0 {
		n
	}
    	else {
		print(n);
		f(n - 1);
	}
};
f(3);
# prints: 3 2 1

f = (a) => {
    print(a);
    ff = (b) => print(a, b);
};
f(1)(2);
# prints: 1 1 2

{ (a) => print(a + a) }(2);
# prints: 4
# value that block returns can be called

{ (a) => { () => (b, c) => { print(a); print(b); print(c) } } }(1)()(2,3);
# { () => (b, c) => { print(1); print(b); print(c) } }()(2,3)
# { (b, c) => { print(1); print(b); print(c) } }(2,3)
# { print(1); print(2); print(3) }
# prints: 1 2 3

f = () => {
    print(1);
    break;
    print(2);
}
f();
# returns: 1
# prints: 1
```

### Loops
Loops also support blocks as decision conditions. If the expression following the `loop` keyword evaluates to true, the block will execute.

**Examples**:
```py
a = 0;
loop a<2 { a:=a+1; print(a) };
# prints: 1 2

loop {a<2} { a:=a+1; print(a) };
# prints: 1 2

a = 2;
loop {a := a - 1; a >= 0} { print(a) };
# prints: 1 0

a = 2;
loop {a := a - 1; print(a); a >= 0} { };
# prints: 1 0

a = -1;
loop {a := a - 1; a >= 0} { print(a) };
# prints: null

a = -1;
loop {a := a - 1; print(a); a >= 0} { };
# prints: -2

{
	a = 0;
	loop {a := a + 1; a <= 5} { a };
}
# returns: 5
# 5 is the last value of a, that is last statement of the loop block

{
    a = true;
    loop a {
        a := false;
        print(11);
        break;
        print(12);
    };
    print(2);
}
# prints: 11 2

{
	loop true { break };
}
# throws: Loop exceeded loop count limit
# `break 1` exist only first block, so in this case it behavies like `continue` from C like languages

{
	loop true break;
}
# returns: null
# break is not in block, so it will exit loop

{
	loop true { print(1); break 2; print(2); };
	print(3);
}
# prints: 1 3
# block returns: 1
# `break 2` exits from block, and exits from loop

{
	loop true { print(1); break 3; print(2); };
	print(3);
}
# prints: 1
# block returns: 1
# `break 2` exits from block, and exits from loop, and exits from block, so print(3) is not executed
 
```

### Other Examples
```py
# order of operations
2 + 2 * 2       # 6
{ 2 + 2 } * 2   # 8 – here a block was used, which executed first and returns: 4
```

```py
# fibonacci
fibonacci = (n) => {
    if n <= 1 {
		n
	}
	else {
		fibonacci(n - 1) + fibonacci(n - 2)
	}
};
n = 10;
print("Fibonacci(" + n + ") = " + fibonacci(n));
```

```py
# "lists"

lNode = (lCurrent, lNext) => {
	(selector) => if selector {lCurrent} else {lNext};
};

lCurrent	= (list) => list(true);
lNext		= (list) => list(false);
isEmpty		= (list) => list == null;

getElement = (list, index) => {
    loop -isEmpty(list) {
        if index == 0 {
			lCurrent(list)		# Found element
		}
		else {
			list = lNext(list);
			index = index - 1;
		}
    }
};

setElement = (list, index, value) => {
    if -isEmpty(list) {
		if index == 0 {
			lNode(value, lNext(list))  					# Found element, so set its value
		}
		else {
			lNode(lCurrent(list), setElement(lNext(list), index - 1))	# Recursive call for next element, and construct new node
		}
	}
};

getLength = (list) => {
	count = 0;
	loop -isEmpty(lst) {
		lst = lNext(lst);
		count = count + 1;
	}
};

list = lNode(10, null);
list = lNode(20, list);
list = lNode(30, list);
# the list now looks like: 30 20 10

print(lCurrent(list))  ;		# prints: 30
print(lCurrent(lNext(list)));  		# wypisze 20

print(get(list, 1));			# prints: 20

print(getLength(list));			# prints: 3

i = 0;
loop i < 3 {
	print(get(list, i));
	i = i + 1;
};
# prints 30 20 10

setElement(list, 1, 69);
i = 0;
loop i < 3 {
	print(get(list, i));
	i = i + 1;
};
# prints 30 69 10
```


```py
# bubble sort

bubbleSort = (list) => {
    swapped = true;
    length = getLength(list);
    
    loop swapped {
        swapped = false;
        i = 0;
        loop i < length - 1 {
            if getElement(list, i) > getElement(list, i + 1) {
                temp = getElement(list, i);
                list = setElement(list, i, getElement(list, i + 1));
                list = setElement(list, i + 1, temp);
                swapped = true;
            };
            i = i + 1;
        }
    };
    list;
};

list = lNode(3, null);
list = lNode(1, list);
list = lNode(4, list);
list = lNode(2, list);
list = lNode(5, list);
# list: 5 2 4 1 3
bubbleSort(list);
# list: 1 2 3 4 5
```

### EBNF Notation
For clarity, spaces are omitted in the EBNF notation.  
The resulting grammar has been empirically tested using the [EBNF Tester](https://mdkrajnak.github.io/ebnftest/).

#### Syntax Part
```ebnf
program		= statements eos;

block		= "{" [statements [eos]] "}"
statements	= {statement eos } statement;
statement	= assign
			| lambda
			| condition
			| loop
			| break
			| expr;

assign		= identifier op_asign statement;
lambda		= "(" args ")" "=>" statement;
func_call	= identifier "(" args ")" { "(" args ")" } | block "(" args ")" { "(" args ")" };
condition	= "if" expr statement { "else" "if" expr statement } ["else" statement];
loop		= "loop" expr statement;
break		= "break" [ statement ];

args		= [{ expr "," } expr];

expr		= ex_and { op_or ex_and };
ex_and		= ex_com { op_and ex_com };
ex_com		= ex_rel [ op_comper ex_rel ];
ex_rel		= ex_add { op_check ex_add };
ex_add		= ex_mul { op_add ex_mul };
ex_mul		= ex_urn { op_mul ex_urn };
ex_urn		= factor | "-" factor;

factor		= int
		| string
		| bool
		| null
		| identifier
		| func_call
		| block;

```
#### Lexical Part
```ebnf
eos			= ";";
int			= (no_zero_digit { digit }) | '0';
string			= "\"" { symbol } "\"";
bool			= "false" | "true";
null			= "null";

op_or			= "||";
op_and			= "&&"
op_comper		= "==" | "!=" | "<" | "<=" | ">" | "<=";
op_check		= "??"
op_add			= "+" | "-";
op_mul			= "*" | "/";

op_asign		= "=" | "?=" | ":=";

identifier		= letter { letter | digit };

symbol			= digit | letter;
digit			= #'[0-9]';
no_zero_digit		= #'[1-9]';
letter			= #'[A-Za-z]';
```

### Error Handling
Errors follow the format: ERROR:line message

- Reference to an undeclared value
	```py
	1. b = a + 1;
	ERROR[1, 5]: "a" was not defined
	```
	
- Attempt to call an expression that is not a function  
	```py
	1. a = 3;
	2. a();
	ERROR[1, 1]: "a" is not callable
	```

- Incorrect number of method arguments  
	```py
	1. f = (a){};
	2. f();
	ERROR[2,1]: "f" expected 1 arguments, but received 0
	```
	
- Syntax error  
	```py
	1. print = 2;
	ERROR[1,3]: Syntax expected "(", but recived "="
	2. a = 2
	3. b = 3;
	ERROR[3,1]: Syntax expected ";", "||"..., but recived "b"
	```

- Invalid operator
	```py
	1. "a" + ()=>{};
	ERROR[1,5]: Operator '+' expected 'string', 'int', but recive callable
	1. "a" * "b";
	ERROR[1, 3]: Operator '*' expected 'int' but recived 'string'
	```

## Running the Program

The program reads from the stream, interprets the code contained within it, and executes it.
<br>The result of the program's execution is printed to the standard output (console).

### Simple run
In root directory `dotnet run FILE_PATH`.
<br>When path not specified program will use `CodeExamples\Test.txt`

### Build and run
Interpreter can be built by `dotnet build`.
<br>It will create an executable file, that can be run by `dotnet .\BlockScript.dll FILE_PATH`.

### Advance usages
The main program is located in the `Program.cs` file.
In `Program.cs`, a `StreamReader` is declared using the statement `using TextReader reader =`, which can be assigned any type of stream.
By default, it is set to `new StreamReader("CodeExamples/Test.txt");`, which means it reads from the specified file.

### Example:
input:
```py
print("hello world");
2 + 2;
```
output:
```
hello world
Execution result: 4 
```

## Project Structure

The project consists 3 main modules:

- **Lexer**
- **Parser**
- **Interpreter**

They depend on each other hierarchically:

- The **Lexer** does **not** depend on the Parser or Interpreter.
- The **Parser** depends only on the data structures of the Lexer.
- The **Interpreter** depends only on the data structures of the Lexer and Parser.

Corresponding to these project files, the second solution `BlockScript.Tests` contains tests:

- `LexerTests`
- `ParserTests`
- `InterpreterTests`
- `UtilitiesTest`
- `IntegrationTests`

---

## Lexer

Lazily converts a text stream into tokens.

**Input**: a text stream
<br>**Provides**: a `GetToken` method that returns the next token.

Each **Token** consists of:

- `Type` (`TokenType`) — an enum containing token types, e.g. `EndOfText`, `OperatorEqual`, `String`.
- `Value` (`IFactorValue`) — a properly parsed value (`IntFactor`, `StringFactor`, `BoolFactor`, `NullFactor`).
- `Position` (`Position`) — line and column number where the token begins.

---

## Parser

Builds a program tree from tokens.

**Input**: a method returning the next token.
<br>**Provides**: a `ParserProgram` method that returns the program tree as a `Block` record.

Defines 3 categories of data types:

### Statement

- `Assign`
- `NullAssign`
- `Declaration`
- `Condition`
- `Lambda`
- `Loop`

### Expression

#### Arithmetical

- `Add`
- `Subtract`
- `Multiply`
- `Divide`

#### Comparison

- `Equals`
- `NotEquals`
- `Greater`
- `GreaterEquals`
- `Less`
- `LessEquals`

#### Logical

- `Not`
- `NullCoalescing`

### Factor

- `ConstFactor`
- `VariableFactor`
- `FunctionCall`
- `Block`

---

## Interpreter

Executes the program tree.

**Input**: the root of the tree as a `Block`, and a list of `built-in functions`.
<br>**Provides**: the `ExecuteProgram` function which returns the result of the program.

A `built-in function` is a predefined method that can be supplied to the interpreter, enabling programs to invoke it during execution.
<br>Users can define their own built-in functions by creating a class that inherits from the `BuildInMethod` base class and implementing its behavior.
<br>The **Interpreter** module includes two such functions:

- `PrintMethod` – prints a single argument to the console.
- `DebugMethod` – adds a single argument to a configurable debug list.

## Testing

The project uses **xUnit** and **FluentAssertions** as testing libraries.
Test coverage: 91%

### Unit Tests

Unit tests cover individual methods in each core component (Lexer, Parser, Interpreter).  
<br>They verify edge cases, exceptions, and sample inputs to ensure correct method behavior in isolation.
- **Lexer tests** – validate that a given text stream is correctly tokenized into the expected sequence of tokens.
- **Parser tests** – verify that a sequence of tokens is accurately transformed into the corresponding abstract syntax tree (program structure).
- **Interpreter tests** – ensure that executing a program tree produces the correct output.
<br>e.g.
```csharp
[Fact]
public void Interpreter_ShouldExecuteLogicOrExpression_AndNotExecuteRightExpression_WhenLeftIsTrue()
{
    // Arrange
    var output = new List<IFactorValue>();
    List<IStatement> program =
    [
        new LogicOrExpression(ConstFactor(true), AddToOutput(ConstFactor(false))),
    ];

    // Act
    var result = ExecuteProgram(program, [new DebugMethod(output)]);

    // Assert
    result .Should().Be(new BoolFactor(true));
    output.Should().BeEmpty();
}
```

### Integration Tests

Integration tests verify the interaction between different components of the project.
<br>Project includes End-To-End (`End2End`) tests, that takes a text stream as input and assert the expected output produced by executing the program.
<br>e.g.
```csharp
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
    returnValue.Should().Be(new IntFactor(55));
    debug.Should().BeEquivalentTo([
        new IntFactor(1),
        new IntFactor(13),
        new IntFactor(55),
    ]);
}
```
