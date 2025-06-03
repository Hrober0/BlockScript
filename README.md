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
print(c);       # prints 2
f();
print(c);       # prints 2, because new c was declared in local context

d = 2;
f = () => d := 3;
print(d);       # prints 2
f();
print(d);       # prints 3, because c value was overided

e := 2;         # error e was not defined
e ?= 2;         # error e was not defined
e = 2;          # works
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
An important aspect of the language to understand is that a **block** consists of consecutive lines of code, resembling the body of a function.

- The entire program is a block.  
- A block has its own **context**, meaning its own local memory, which contains:  
  - all declared variables (including functions)  
  - a reference to the **context** of the parent function  
- Blocks can be nested.  
- Each block returns a value equal to the value of the last instruction in the block.  
- The `;` symbol is used to separate instructions from one another; it may or may not appear at the end of a block.  
<br>**Examples**:

```py
{
a=3;
b=4;
}
# blok returns: 4

{
a=3;
b={4};		# the same as b=4;
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
# blok returns: lambda ()=>{a>b}

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

if a==1 {
	1
}
else if a==2 {
	2
}
else {
	"no"
};
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
```

### Loops
Loops also support blocks as decision conditions. If the expression following the `loop` keyword evaluates to true, the block will execute.

**Examples**:
```py
a = 0;
loop a<2 { a=a+1; print(a) };
# prints: 1 2

loop {a<2} { a=a+1; print(a) };
# prints: 1 2

a = 2;
loop {a = a - 1; a >= 0} { print(a) };
# prints: 1 0

a = 2;
loop {a = a - 1; print(a); a >= 0} { };
# prints: 1 0

a = -1;
loop {a = a - 1; a >= 0} { print(a) };
# prints: null

a = -1;
loop {a = a - 1; print(a); a >= 0} { };
# prints: -2

{
	a = 0;
	loop {a = a + 1; a <= 5} { a };
}
# returns: 5 – because this is the last value of the block
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
			lNode(value, lNext(list))  									# Found element, so set its value
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
print(lCurrent(lNext(list)));  	# wypisze 20

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
			| expr;

assign		= identifier op_asign statement;
lambda		= "(" args ")" "=>" statement;
func_call	= identifier "(" args ")";
condition	= "if" expr statement { "else" "if" expr statement } ["else" statement];
loop		= "loop" expr statement;

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
eos				= ";";
int				= (no_zero_digit { digit }) | '0';
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
no_zero_digit	= #'[1-9]';
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
