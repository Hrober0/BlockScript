# BlockScript

## Wstęp
BlockScript jest językiem ogólnego przeznaczenia, nastawiony na obsługę funkcji w zawansowanym kontekście.
Jest dynamicznie typowany, słabymi typami, posiada domyślnie mutowalne zmienne oraz parametry przekazywane przez kopie.
Wspiera działanie funkcji wyższego rzędu, zagnierzdżanie ich oraz przekzywanie jako parametr.
Język wyróżnia szerokie zastosowanie bloku czyli części kodu, mogącego być użyty jako wartości warunkowe lub parametry.

## Zasady działania języka

### Typy danych
- int
- bool
- string
- null

### Operatory arytmetyczno logiczne
Ułożone malejąco według priorytetu
- ! - negacja
- * - mnożenie
- / - dzielenie
- + - dodawanie
- - - odejmowanie
- ?? - wartość nie null
- > - większe
- >= - większe lub równe
- < - mniejsze
- <= - mniejsze lub równe
- == - równe
- != - nie równe
- && - i
- || - lub

### Operatory przypisania
- = wrzypisanie
- =? przypisanie warunkowe

### Zmienne
- Zmienna jest określonego typu, ale typ zmiennej może się zmieniać w czasie.
- Operacje na zmiennych różnego typu nie wymagają jawnego żutowania na ten sam typ.
a = 4;
a = "a"; # a zmieni typ z int na string

np.
a = 4;
b ?= 3; # b zostanie ustawione na 3 ponieważ jest nullem
b ?= 4; $ b pozostanie z wartością 3 
b = a ?? 5; # b zostanie ustawione na 4

### Komentarze
/# komentarz

### Blok
Ważny do zrozumienia aspekt języka, blok stanowią kolejne linijki kodu, przypominjąc ciało funkcji.
Cały program stanowi blok.
Blok ma swoją własną lokalną pamięć.
Bloki mogą być zagnieżdżane.
Blok ma dostęp do pamięci bloku rodzica.
Każdy blok zwraca wartość równą wartości z ostaniej instrukcji w bloku.
Symbol ; jest użyty do odseparowania instrukcji od siebie, może ale nie musi występować na końcu bloku
np.
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
a>b			# ostatnie instrukcja nie wymaga ;
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

### Instrukcje warunkowe
Instrulcje warunkowe przypominają ternary operator

{a>3}?{print{a}};				# gdy a>3 wypisze a i zwróci a w przeciwnym razie zwróci null;

{a>3}?{print{a}}:{"no"}; 		# gdy a>3 wypisze a i zwróci a w przeciwnym razie zwróci "no";

{a>3||a<2}?{print{a};a+1};		# gdy a>3 lub a<2 wypisze a i zwróci a+1 w przeciwnym razie zwróci null;

2 + 2 * 2 > 7 || {{3>2}?{1}} 	# => {2 + 4 > 7 || 1} => {6 > 7 || 1} => {false || true} => true

### Funkcje
f=(){print{"a"}};
f();								# wypisze w konsoli "a"

{(){print{"a"}}}();					# wypisze w konsoli "a"

f=(a){print{a}};
f("b");								# wypisze w konsoli "b"

### Pętle
Pętle również wspierają bloki jako jako warunek deyzyjny
a = 0;
loop a<10 {a=a+1;print a};

loop a?=10; a=a-1 {print a};

### Notacja EBNF
Dla przejrzystości w notacji EBNF pomijam znak spacji.
Powstała gramatyka została przetestowana empirycznie za pomocą narzędzia [EBNF Tester](https://mdkrajnak.github.io/ebnftest/).

#### Część składniowa

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
loop		= "loop" statements block;
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

#### Część leksykalna

eos			= ";";
int			= digit { digit };
string		= "\"" { symbol } "\"";
bool		= "false" | "true";
null		= "null";

op_logical	= "&&" | "||";
op_comper	= "==" | "!=" | "<" | "<=" | ">" | "<=";
op_check	= "??"
op_add		= "+" | "-";
op_mul		= "*" | "/";

op_asign	= "=" | "?=";

args		= [{ expr "," } expr];
identifier = letter { letter | digit };

symbol		= digit | letter;
digit		= #'[0-9]';
letter		= #'[A-Za-z]';


### Obsługa błędów
Błędy przyjmują format: ERROR:line komuntikat

- Odwołanie się do nie zadeklarowanej wartości
	1. b = a + 1;
	ERRPR:1 "a" was not defined
	
- Próba wywołania wyrażenia niebędącego funkcjią
	1. a = 3;
	2. a();
	ERROR:2 "a" is not callable

- Nieprawidłowa ilość argumentów metody
	1. f = (a){};
	2. f();
	ERROR:2 "f" expected 1 arguments, but received 0
	
- Błąd składni
	1. print = 2;
	ERROR:1 Syntax expected "{", but recived "="
	2. a = 2
	3. b = 3;
	ERROR:2 Syntax expected ";", "||"..., bur recived "b"