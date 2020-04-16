using System;
using System.Collections.Generic;
using System.Linq;

using static Keeper.BacktraQ.Query;

namespace Keeper.BacktraQ
{
    class Program
    {
        static void Main(string[] args)
        {
            SimpleUnifyVariable();
            UnifyTwoVariables();
            UnifyOptions();
            GetListMembers();
            ShuffleList();
            Negation();
            Arithmetic();
            SimpleDcg();
            DcgState();
            QueryTimeCodeExecution();
        }

        private static void SimpleUnifyVariable()
        {
            DisplayHeader("Simple Unify Variable");

            // Create unbound variable
            var variable = new Var<int>();

            // Create query as "unify variable with 123"
            var query = variable <= 123;
            // This is a shorthand for "var query = variable.Unify(123);"

            // Run query and display all results
            int count = 0;

            foreach (var result in query)
            {
                Console.WriteLine($"variable = {variable}");
                count++;
            }

            Console.WriteLine($"Result count: {count}");
        }

        private static void UnifyTwoVariables()
        {
            DisplayHeader("Unify Two Variables");

            // Create unbound variables
            var variable1 = new Var<int>();
            var variable2 = new Var<int>();

            // Create query as "unify variable1 with variable2, and unify variable2 with 123"
            var query = variable1 <= variable2 & variable2 <= 123;
            // This is a shorthand for "var query = (variable1 <= variable2).And(variable2 <= 123);"

            // Run query and display all results
            int count = 0;

            foreach (var result in query)
            {
                Console.WriteLine($"variable1 = {variable1}");
                Console.WriteLine($"variable2 = {variable2}");
                count++;
            }

            Console.WriteLine($"Result count: {count}");
        }

        private static void UnifyOptions()
        {
            DisplayHeader("Unify Options");

            // Create unbound variable
            var variable = new Var<char>();

            // Create query as "unify variable with 'a', or 'b', or 'c'"
            var query = variable <= 'a' | variable <= 'b' | variable <= 'c';
            // This is a shorthand for "var query = (variable <= 'a').Or(variable <= 'b').Or(variable <= 'c');"

            // Run query and display all results
            int count = 0;

            foreach (var result in query)
            {
                Console.WriteLine($"variable = {variable}");
                count++;
            }

            Console.WriteLine($"Result count: {count}");
        }

        private static void GetListMembers()
        {
            DisplayHeader("Get List Members");

            // Create unbound variables
            var member = new Var<int>();
            var length = new Var<int>();

            // Create pre-populated list
            var list = VarList.Create(1, 2, 3, 4);

            // Create query as "unify member with each member of the list"
            var query = member <= list.Member;
            // This is a shorthand for "var query = list.Member(member);"

            // Run query and display all results
            int count = 0;

            foreach (var result in query)
            {
                Console.WriteLine($"member = {member}");
                count++;
            }

            Console.WriteLine($"Result count: {count}");

            // Create query as "unify member with each member of the list"
            var query2 = length <= list.Length;
            // This is a shorthand for "var query = list.Member(member);"

            // Run query and display all results
            count = 0;

            foreach (var result in query2)
            {
                Console.WriteLine($"length = {length}");
                count++;
            }

            Console.WriteLine($"Result count: {count}");
        }

        private static void ShuffleList()
        {
            DisplayHeader("Shuffle List");

            // Create unbound variable
            var member = new Var<int>();

            // Create pre-populated list
            var list = VarList.Create(1, 2, 3, 4);

            // Create query as "unify member with each member of the list in a random order"
            var query = member <= list.RandomMember;

            // Run query and display all results
            int count = 0;

            foreach (var result in query)
            {
                Console.WriteLine($"member = {member}");
                count++;
            }

            Console.WriteLine($"Result count: {count}");
        }

        private static void Negation()
        {
            DisplayHeader("Negation");

            // Create unbound variable
            var member = new Var<int>();

            // Create pre-populated lists
            var list = VarList.Create(1, 2, 3, 4);
            var list2 = VarList.Create(3, 4, 5, 6);

            // Create query as "unify member with each member of the list, and
            // don't unify with members of list2"
            var query = member <= list.Member & !(member <= list2.Member);
            // This is a shorthand for "var query = member <= list.Member & Query.Not(member <= list2.Member);"

            // Run query and display all results
            int count = 0;

            foreach (var result in query)
            {
                Console.WriteLine($"member = {member}");
                count++;
            }

            Console.WriteLine($"Result count: {count}");
        }

        private static void Arithmetic()
        {
            DisplayHeader("Arithmetic");

            var x = new Var<int>();
            var y = new Var<int>();
            var z = new Var<int>();

            var firstQuery = x.Between(1, 4)
                                & y.Between(5, 8)
                                & z <= x.Add(y);

            var secondQuery = x.Between(1, 4)
                                & z.Between(5, 8)
                                & z <= x.Add(y);

            // Run query and display all results
            int count = 0;

            Console.WriteLine($"x + y = z?");

            foreach (var result in firstQuery)
            {
                Console.WriteLine($"{x} + {y} = {z}");
                count++;
            }

            Console.WriteLine($"Result count: {count}");

            count = 0;

            Console.WriteLine();
            Console.WriteLine($"x + y? = z");

            foreach (var result in secondQuery)
            {
                Console.WriteLine($"{x} + {y} = {z}");
                count++;
            }

            Console.WriteLine($"Result count: {count}");
        }

        private static void SimpleDcg()
        {
            DisplayHeader("Simple DCG");

            // Create a list of phrase options
            var programmingLanguage = (Phrase)"Prolog" ^ "C#" ^ "Lisp" ^ "C++" ^ "Java";

            // Create a randomised list of OSes
            var os = Phrase.RandomPhrase("Windows", "Linux", "MacOS");

            // Build a phrase from combined parts
            var sentence = "I write " + programmingLanguage + " on a " + os + " box.";

            // Create a string variable
            var sentenceText = new Var<string>();

            // Create query as "render the 'sentence' phrase to sentenceText"
            var query = sentenceText <= sentence.AsString;

            // Run query and display all results
            int count = 0;

            foreach (var result in query)
            {
                Console.WriteLine($"sentenceText = {sentenceText}");
                count++;
            }

            Console.WriteLine($"Result count: {count}");
        }

        private static void DcgState()
        {
            DisplayHeader("DCG State");

            // Map numbers to text
            var numberPart = Phrase.SwitchPhrase((0, "zero"),
                                                    (1, "one"),
                                                    (2, "two"),
                                                    (3, "three"),
                                                    (4, "four"),
                                                    (5, "five"),
                                                    (6, "six"),
                                                    (7, "seven"),
                                                    (8, "eight"),
                                                    (9, "nine"));

            // Create phrase parts for singular & plural
            var suffixPart = Phrase.SwitchPhrase((false, ""), (true, "s"));

            var verbPart = Phrase.SwitchPhrase((false, "is"), (true, "are"));

            // Build a phrase from the parts
            // Check the phrase count and the number-tense match
            Phrase sentence(Var<int> itemCount) => "There " + verbPart(NewVar<bool>(out var isPlural)) + " " + numberPart(itemCount) + " item" + suffixPart(isPlural) + "." + Map(itemCount, isPlural, countValue => countValue != 1);
            
            var rng = new Random();
            var sentenceText = new Var<string>();

            // Create query as "render the 'sentence' phrase for a random number of items to sentenceText"
            var query = sentenceText <= sentence(rng.Next(1, 10)).AsString;
            
            // Run query and display all results
            int count = 0;

            foreach (var result in query)
            {
                Console.WriteLine($"sentenceText = {sentenceText}");
                count++;
            }

            Console.WriteLine($"Result count: {count}");
        }

        private static void NaturalGrammar()
        {
            DisplayHeader("Natural Grammar");
            
        }

        private static void QueryTimeCodeExecution()
        {
            DisplayHeader("Query-time Code Execution");

            // Create unbound variable
            var member = new Var<int>();

            // Create pre-populated lists
            var list = VarList.Create(1, 2, 3, 4);
            var list2 = VarList.Create(3, 4, 5, 6);

            // Create query as "unify member with each member of the list,
            // output the value of member at this point in the query execution,
            // and don't unify with members of list2"
            var query = member <= list.Member
                            & (() =>
                                {
                                    Console.WriteLine($"  try with member = {member}");

                                    return Query.Success;
                                })
                            & !(member <= list2.Member);

            // Run query and display all results
            int count = 0;

            foreach (var result in query)
            {
                Console.WriteLine($"member = {member}");
                count++;
            }

            Console.WriteLine($"Result count: {count}");
        }

        private static void DisplayHeader(string header)
        {
            Console.WriteLine();
            Console.WriteLine(header);
            Console.WriteLine(new string('-', header.Length));
        }
    }
}
