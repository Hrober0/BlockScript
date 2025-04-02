# BlockScript

## Wstęp
BlockScript to język ogólnego przeznaczenia, ukierunkowany na obsługę funkcji w zaawansowanym kontekście.  
Jest dynamicznie typowany z domyślnie mutowalnymi zmiennymi oraz parametrami przekazywanymi przez kopię.  
Wspiera funkcje wyższego rzędu, zagnieżdżanie funkcji oraz przekazywanie ich jako parametry.  

Język wyróżnia szerokie zastosowanie **bloku**, czyli części kodu, która może być użyta jako wartość warunkowa lub parametr.

## Zasady działania języka

### Typy danych
- `int`
- `bool`
- `string`
- `null`

### Operatory arytmetyczno-logiczne
_(Ułożone malejąco według priorytetu)_
- `!`  – negacja
- `*`  – mnożenie  
- `/`  – dzielenie  
- `+`  – dodawanie  
- `-`  – odejmowanie  
- `??` – wartość nie-null  
- `>`  – większe  
- `>=` – większe lub równe  
- `<`  – mniejsze  
- `<=` – mniejsze lub równe  
- `==` – równe  
- `!=` – nierówne  
- `&&` – i  
- `||` – lub

### Wspierane typy danych dla operatorów
- bool: `||`, `&&`, `!`
- int: `+`, `-`, `*`, `/`, `>`, `>=`, `<`, `<=`
- string: `+`
- bool, int, string, null: `??`, `==`, `!=`
<br><br>_(Dla innych typów program zwróci błąd)_

### Operatory przypisania
- `=`  – przypisanie  
- `=?` – przypisanie warunkowe  

## Zmienne
- Zmienna ma określony typ, ale może się on zmieniać w czasie.  
- Operacje na zmiennych różnych typów nie wymagają jawnego rzutowania.  

**Przykłady**:
```blockscript
a = 4;
a = "a";		# a zmieni typ z int na string

b ?= 3;			# b zostanie ustawione na 3, bo jest nullem

b ?= 4;			# b pozostanie 3

b = a ?? 5;		# b zostanie ustawione na 4
```

**Konwwersje typów**:

```blockscript
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

### Komentarze
\# komentarz

### Blok
Ważny do zrozumienia aspekt języka, blok stanowią kolejne linijki kodu, przypominjąc ciało funkcji.
Cały program stanowi blok.
Blok ma swoją własną lokalną pamięć.
Bloki mogą być zagnieżdżane.
Blok ma dostęp do pamięci bloku rodzica.
Każdy blok zwraca wartość równą wartości z ostaniej instrukcji w bloku.
Symbol ; jest użyty do odseparowania instrukcji od siebie, może ale nie musi występować na końcu bloku
**Przykłady**:
```blockscript
{
a=3;
b=4;
}
powyższy blok zwróci: 4

{
a=3;
b={4};		# to samo co b=4;
a+b;
}
powyższy blok zwróci: 7

{
a=3;
b=4;
a>b		# ostatnie instrukcja nie wymaga ;
}
powyższy blok zwróci: false

{
a=3;
b=4;
(){a>b};
}
powyższy blok zwróci: (){false}

{
print{"a"};
}
powyższy blok zwróci: "a"
```

### Instrukcje warunkowe
Instrulcje warunkowe przypominają ternary operator

**Przykłady**:
```blockscript
{a>3}?{print{a}};
# gdy a>3 wypisze a i zwróci a w przeciwnym razie zwróci null;

{a>3}?{print{a}}:{"no"};
# gdy a>3 wypisze a i zwróci a w przeciwnym razie zwróci "no";

{a>3||a<2}?{print{a};a+1};
# gdy a>3 lub a<2 wypisze a i zwróci a+1 w przeciwnym razie zwróci null;

2 + 2 * 2 > 7 || {{3>2}?{1}}
# {2 + 4 > 7 || 1}
# {6 > 7 || 1}
# {false || true}
# true

a = 1
{2 > a} ? {print{"ok"}} : {print{"no"}}
# wypisze ok

a = 1
print{{2 > a} ? {"ok"} : {"no"}}
# to samo co wyżej zapisane inaczej

a = 3
print{{2 > a} ? {"ok"}}
# wypisze null

a = "a"
print{{a} ? {"ok"} : {"no"}}
# wypisze ok - ponieważ string sparsowany będzie na bool dając true

a = ""
print{{a} ? {"ok"} : {"no"}}
# wypisze no - ponieważ string sparsowany będzie na bool dając false

a = 3
print{{a} ? {"ok"} : {"no"}}
# wypisze ok - ponieważ int sparsowany będzie na bool dając true

a = 0
print{{a} ? {"ok"} : {"no"}}
# wypisze no - ponieważ int sparsowany będzie na bool dając false
```

### Funkcje

**Przykłady**:
```blockscript
f=(){print{"a"}};
f();
# wypisze w konsoli "a"

{(){print{"a"}}}();
# wypisze w konsoli "a"

f=(a){print{a}};
f("b");
# wypisze w konsoli "b"
```

### Pętle
Pętle również wspierają bloki jako jako warunek deyzyjny

**Przykłady**:
```blockscript

# odpowednik pętli while
a = 0;
loop {a=a+1; print{a}; a<10};

# odpowednik pętli for
loop {a?=10; a=a-1; print{a}; a>10};
# a nie istniało więc zostanie za pierwszym razem ustawione na 10
# funckja wypisze kolejno 9 8 7 6 5 4 3 2 1 0
# gdy a==0 warunek na końcu bloku a>0 zwróci fałsz więc pętla się zakończy
```

### Inne przykłady
```blockscript
# kolejnośc działań
2 + 2 * 2		# 6
{ 2 + 2 } * 2	# 8 - tutaj został użyty blok w któr wykonał się najpier zwracając 4
```

```blockscript
# fibonacci
fibonacci = (n) => {
    {n <= 1}
	? {n}
    : {fibonacci(n - 1) + fibonacci(n - 2)}
};
n = 10;
print{"Fibonacci(" + n + ") = " + fibonacci(n)};
```

```blockscript
# "lists"

cons = (head, tail) => {
	(selector) => {selector} ? {head} : {tail}
};

head = (list) => { list(true) };
tail = (list) => { list(false) };
isEmpty = (list) => { list == null };

getElement = (list, n) => {
    { !isEmpty(list) } 						# List is not empty
	? {
		{n == 0}
		? { head(list) }  					# Found element
		: { getElement(tail(list), n - 1) }	# Recursive call for next element
	}  
};

getLength = (list) => {
	{isEmpty(list)}
	? {0}
	: {
		count = 0;
		loop {
			count = count + 1;
			lst = tail(lst);
			!isEmpty(lst)
		}
		count
	}
};

list = cons(10,				# pierwszy element
		cons(20,			# drugi element
		cons(30,			# trzeci element
		null)))				# koniec listy

print{head(list)}  			# wypisze 10
print{head(tail(list))}  	# wypisze 20

print{get(list, 1)}			# wypisze 20

print{getLength(list)}		# wypisze 3

loop {
	i?=0;
	print{get(list, i)}
	i+=1;
	i < 2;
}
# wypisze 10 20 30
```


```blockscript
# bubble sort

bubbleSort = (list) => {
    length = getLength(list)

	loop {
		i ?= 0;

		current = list
        newList = null
        prev = null

		loop {

			first = head(current)
            second = head(tail(current))

            {first > second}
			? {
                # Swap elements
                newPair = cons(second, cons(first, tail(tail(current))))
                newList = isEmpty(prev) ? {newPair} : {prev(false) = newPair}
            } : {
                # Keep order
                newPair = cons(first, tail(current))
                newList = isEmpty(prev) ? {newPair} : {prev(false) = newPair}
            }

            prev = tail(current)
            current = tail(current)

			!isEmpty(tail(current))
		}
        
        list = newList

		i += 1;
		i < length;
	}
	list
}

list = cons(40,				# pierwszy element
		cons(20,			# drugi element
		cons(30,			# trzeci element
		cons(41,			# czwarty element
		null))))			# koniec listy
bubbleSort(list)

```

### Notacja EBNF
Dla przejrzystości w notacji EBNF pomijam znak spacji.
Powstała gramatyka została przetestowana empirycznie za pomocą narzędzia [EBNF Tester](https://mdkrajnak.github.io/ebnftest/).

#### Część składniowa
```ebnf
program		= statements eos;

block		= "{" [statements [eos]] "}"
statements	= {statement eos } statement;
statement	= assign
			| lambda
			| func_call
			| condition
			| loop
			| print
			| expr;

assign		= identifier op_asign expr;
lambda		= "(" args ")" block;
func_call	= (identifier | block) "(" args ")";
condition	= block "?" block [":" block];
loop		= "loop" block;
print		= "print" block;

expr		= ex_com { op_logical ex_com };
ex_com		= ex_rel { op_comper ex_rel };
ex_rel		= ex_add { op_check ex_add };
ex_add		= ex_mul { op_add ex_mul };
ex_mul		= ex_urn { op_mul ex_urn };
ex_urn		= factor | "!" factor;

factor		= int
			| string
			| bool
			| null
			| identifier
			| statement
			| block;
```
#### Część leksykalna
```ebnf
eos				= ";";
int				= (no_zero_digit { digit }) | '0';
string			= "\"" { symbol } "\"";
bool			= "false" | "true";
null			= "null";

op_logical		= "&&" | "||";
op_comper		= "==" | "!=" | "<" | "<=" | ">" | "<=";
op_check		= "??"
op_add			= "+" | "-";
op_mul			= "*" | "/";

op_asign		= "=" | "?=";

args			= [{ expr "," } expr];
identifier		= letter { letter | digit };

symbol			= digit | letter;
digit			= #'[0-9]';
no_zero_digit	= #'[1-9]';
letter			= #'[A-Za-z]';
```

### Obsługa błędów
Błędy przyjmują format: ERROR:line komuntikat

- Odwołanie się do nie zadeklarowanej wartości
	```blockscript
	1. b = a + 1;
	ERRPR:1 "a" was not defined
	```
	
- Próba wywołania wyrażenia niebędącego funkcjią
	```blockscript
	1. a = 3;
	2. a();
	ERROR:2 "a" is not callable
	```

- Nieprawidłowa ilość argumentów metody
	```blockscript
	1. f = (a){};
	2. f();
	ERROR:2 "f" expected 1 arguments, but received 0
	```
	
- Błąd składni
	```blockscript
	1. print = 2;
	ERROR:1 Syntax expected "{", but recived "="
	2. a = 2
	3. b = 3;
	ERROR:2 Syntax expected ";", "||"..., but recived "b"
	```

- Nieprawidłowy operator
	```blockscript
	1. "a" + (){};
	ERROR:1 Operator '+' expected 'string', 'int', but recive callable
	1. "a" * "b";
	ERROR:1 Operator '*' expected 'int' but recived 'string'
	```
