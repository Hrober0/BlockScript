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
```py
a = 4;
a = "a";		# a zmieni typ z int na string

b ?= 3;			# b zostanie ustawione na 3, bo jest nullem

b ?= 4;			# b pozostanie 3

b = a ?? 5;		# b zostanie ustawione na 4
```

**Konwwersje typów**:

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

### Komentarze
\# komentarz

### Blok
Ważny do zrozumienia aspekt języka, blok stanowią kolejne linijki kodu, przypominjąc ciało funkcji.
- Cały program stanowi blok.
- Blok ma swój **kontekst** czyli własną lokalną pamięć, w któej znajdują się:
	- wszystkie zadeklarowane zmienne (w tym funkcje)
 	- referęcja na **kontekst** funkcji nadrzędnej
- Bloki mogą być zagnieżdżane.
- Każdy blok zwraca wartość równą wartości z ostaniej instrukcji w bloku.
- Symbol ; jest użyty do odseparowania instrukcji od siebie, może ale nie musi występować na końcu bloku
<br>**Przykłady**:
```py
{
a=3;
b=4;
}
# powyższy blok zwróci: 4

{
a=3;
b={4};		# to samo co b=4;
a+b;
}
# powyższy blok zwróci: 7

{
a=3;
b=4;
a>b		# ostatnie instrukcja nie wymaga ;
}
# powyższy blok zwróci: false

{
a=3;
b=4;
()=>{a>b};
}
# powyższy blok zwróci: lambdę ()=>{a>b}

{
print("a");
}
# powyższy blok zwróci: "a"

{
	a = 4;
	c = {
		a = a - 1;
		b = a - 1;
	};
	print(a);
	print(c - 1);
}
# powyższy blok zwróci: 1
# zmienna b nie będzie widoczna w zewnętrznym bloku
# wypisze 3 1
```

### Instrukcje warunkowe
Instrulcje warunkowe przypominają ternary operator

**Przykłady**:
```py
if {a>3} {print(a)};
# gdy a>3 wypisze a i zwróci a w przeciwnym razie zwróci null;

if a>3 {print(a)};
# po if może znaleźć się pojedyńcze wyrażenie, gdy a>3 wypisze a i zwróci a w przeciwnym razie zwróci null;

if a>3 {print(a)} else {"no"};
# gdy a>3 wypisze a i zwróci a w przeciwnym razie zwróci "no";

if a>3||a<2 {print a; a+1};
# gdy a>3 lub a<2 wypisze a i zwróci a+1 w przeciwnym razie zwróci null;

2 + 2 * 2 > 7 || {if 3>2 {1}}
# {2 + 4 > 7 || 1}
# {6 > 7 || 1}
# {false || true}
# true

# true || false && true
# zwróci false, operator && ma pierwszeństwo

a = 1
if 2 > a {print("ok")} else {print("no")}
# wypisze ok

a = 1
print(if 2 > a {"ok"} else {"no"})
# to samo co wyżej zapisane inaczej

a = 3
print(if 2 > a {"ok"})
# wypisze null

a = "a"
print(if a {"ok"} else {"no"})
# wypisze ok - ponieważ string sparsowany będzie na bool dając true

a = ""
print(if a {"ok"} else {"no"})
# wypisze no - ponieważ string sparsowany będzie na bool dając false

a = 3
print(if a {"ok"} else {"no"})
# wypisze ok - ponieważ int sparsowany będzie na bool dając true

a = 0
print(if a {"ok"} else {"no"})
# wypisze no - ponieważ int sparsowany będzie na bool dając false

if a==1 {
	1
}
else if a==2 {
	2
}
else {
	"no"
}
# gdy a==1 zwróci 1, gdy a==2 zwróci 2, w przeciwnym wypadku zwróci "no"
```

### Funkcje

**Przykłady**:
```py
f=()=>{print("a")};
f();
# wypisze w konsoli "a"


f=()=>print("a");
print("b")
f();
# wypisze w konsoli "b" "a"


f=(a)=>print(a);
f("b");
# wypisze w konsoli "b"


f = () => {
	a = 2;
	ff = () => { a };
	print(ff());
	a = 3;
	print(ff());
};
f();
# wypisze 2 3
# funkcja "ff" ma dostęp do kontekstu nadrzędnej funkcji "f", dlatego skożysta z aktualnej wartości zmiennej "a"


f = () => {
	a = 2;
	ff = () => { a = a + 1 };
	a = 3;
	ff;
};
print(f()());	# wypisze 4
print(f()());	# wypisze 4
print(f()());	# wypisze 4
fc = f();
print(fc());	# wypisze 4
print(fc());	# wypisze 5
print(fc());	# wypisze 6
# funkcja "ff" ma dostęp do kontekstu nadrzędnej funkcji "f", mimo że funkcja zostało już wykonana
# dlatego funkcja "ff" użyje ostatniej wartości zmiennej "a" czyli 3 i zwiększy ją o jeden
# "print(f()())" tworzy nowy kontekst funkcji "f" dlatego ponowne wywołanie "print(f()())" zwrócić ponownie 4
# natomiast wykonując "fc = f();" zapisujemy funkcję mającą referęcję na kontekst funkcji "f",
# dlatego wykonując "fc()" wielokrotnie modyfikujemy ten sam kontekst co skutkuje wypisaniem kolejnych wartości


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
# wypisze 3 2 1
```

### Pętle
Pętle również wspierają bloki jako jako warunek deyzyjny, jeśli wyrażenie za słowem kluczowym "loop" będzie prawdziwe blok będzie się wykonywał.

**Przykłady**:
```py

a = 0;
loop a<2 { a=a+1; print(a) };
# wypisze 1 2

loop {a<2} { a=a+1; print(a) };
# wypisze 1 2

a = 2;
loop {a = a - 1; a >= 0} { print(a) };
# wypisze 1 0

a = 2;
loop {a = a - 1; print(a) a >= 0} { };
# wypisze 1 0

a = -1;
loop {a = a - 1; a >= 0} { print(a) };
# nic nie wypisze

a = -1;
loop {a = a - 1; print(a) a >= 0} { };
# wypisze -2

{
	a = 0;
	loop {a = a + 1; a <= 5} { a };
}
# zwróci 5 - ponieważ jest to ostatnia wartość z bloku
```

### Inne przykłady
```py
# kolejnośc działań
2 + 2 * 2		# 6
{ 2 + 2 } * 2	# 8 - tutaj został użyty blok w któr wykonał się najpier zwracając 4
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
print{"Fibonacci(" + n + ") = " + fibonacci(n)};
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
    loop !isEmpty(list) {
        if index == 0 {
			lCurrent(list)		# Found element
		}
		else {
			list = lNext(list);
			index = index - 1;
		}
    }
}

setElement = (list, index, value) => {
    !isEmpty(list)
	? {
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
	loop !isEmpty(lst) {
		lst = lNext(lst);
		count = count + 1;
	}
};

list = lNode(10, null);
list = lNode(20, list);
list = lNode(30, list);
# lista wygląda następująco 30 20 10

print(lCurrent(list))  			# wypisze 30
print(lCurrent(lNext(list)))  	# wypisze 20

print(get(list, 1))				# wypisze 20

print(getLength(list))			# wypisze 3

i = 0;
loop i < 3 {
	print(get(list, i))
	i = i + 1;
}
# wypisze 30 20 10

setElement(list, 1, 69);
i = 0;
loop i < 3 {
	print(get(list, i));
	i = i + 1;
}
# wypisze 30 69 10
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
            }
            i = i + 1;
        }
    }
    list;
};

list = lNode(3, null);
list = lNode(1, list);
list = lNode(4, list);
list = lNode(2, list);
list = lNode(5, list);
# list to 5 2 4 1 3
bubbleSort(list)
# list to 1 2 3 4 5
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
			| condition
			| loop
			| print
			| expr;

assign		= identifier op_asign expr;
lambda		= "(" args ")" "=>" expr;
func_call	= identifier "(" args ")";
condition	= "if" expr block { "if" "else" block } ["else" block];
loop		= "loop" expr block;
print		= "print" "(" expr ")";

args		= [{ expr "," } expr];

expr		= ex_and { op_or ex_and };
ex_and		= ex_com { op_and ex_com };
ex_com		= ex_rel [ op_comper ex_rel ];
ex_rel		= ex_add { op_check ex_add };
ex_add		= ex_mul { op_add ex_mul };
ex_mul		= ex_urn { op_mul ex_urn };
ex_urn		= factor | "!" factor;

factor		= int
			| string
			| bool
			| null
			| identifier
			| func_call
			| block;

```
#### Część leksykalna
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

op_asign		= "=" | "?=";

identifier		= letter { letter | digit };

symbol			= digit | letter;
digit			= #'[0-9]';
no_zero_digit	= #'[1-9]';
letter			= #'[A-Za-z]';
```

### Obsługa błędów
Błędy przyjmują format: ERROR:line komuntikat

- Odwołanie się do nie zadeklarowanej wartości
	```py
	1. b = a + 1;
	ERROR[1, 5]: "a" was not defined
	```
	
- Próba wywołania wyrażenia niebędącego funkcjią
	```py
	1. a = 3;
	2. a();
	ERROR[1, 1]: "a" is not callable
	```

- Nieprawidłowa ilość argumentów metody
	```py
	1. f = (a){};
	2. f();
	ERROR[2,1]: "f" expected 1 arguments, but received 0
	```
	
- Błąd składni
	```py
	1. print = 2;
	ERROR[1,3]: Syntax expected "(", but recived "="
	2. a = 2
	3. b = 3;
	ERROR[3,1]: Syntax expected ";", "||"..., but recived "b"
	```

- Nieprawidłowy operator
	```py
	1. "a" + (){};
	ERROR[1,5]: Operator '+' expected 'string', 'int', but recive callable
	1. "a" * "b";
	ERROR[1, 3]: Operator '*' expected 'int' but recived 'string'
	```
