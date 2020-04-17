# BacktraQ

BacktraQ is a framework for running Prolog-like queries with backtracking, choicepoints & variable unification within managed C#.

The library has no external dependencies and runs as pure managed code, to allow it to be used in limited/sandbox domains like Unity3D.

## Usage

BacktraQ has two stages - building queries, and running them. Building queries is designed to look like a native extension to C# syntax, so logic program code is inline with regular imperative statements. Various operators have been overloaded to support this - for example, `<=` is used as the unification operator.

```c#

// Create unbound variables
var a = new Var<int>();
var b = new Var<int>();

// Create query as "unify a with b, and unify b with 123"
var query = a <= b & b <= 123;

```

The query doesn't execute and no variables are set/unified until you test for success:

```c#

if (query.Succeeds())
{
    Console.WriteLine($"a = {a}");
    Console.WriteLine($"b = {b}");
}

```

If a query may have multiple solutions, it can be iterated over and the value of each variable will update:


```c#

// Create unbound variable
var a = new Var<int>();

// Create query as "unify a with 'a', or 'b', or 'c'"
var query = a <= 'a' | a <= 'b' | a <= 'c';

foreach (var result in query)
{
    Console.WriteLine($"a = {a}");
}

```

## Contributing
Pull requests are welcome!

## License
[MIT](https://choosealicense.com/licenses/mit/)