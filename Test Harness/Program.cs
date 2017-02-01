using System;
using System.Collections.Generic;
using System.Linq;

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
            Negation();
            SimpleDCG();

            Console.ReadLine();
        }

        private static void SimpleUnifyVariable()
        {
            DisplayHeader("Simple Unify Variable");

            // Create unbound variable
            var variable = new Var<int>();

            // Create query as "unify variable with 123"
            var query = variable <= 123;

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

            // Create unbound variable
            var member = new Var<int>();

            // Create pre-populated list
            var list = VarList.Create(1, 2, 3, 4);

            // Create query as "unify member with each member of the list"
            var query = member <= list.Member;

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

            // Create query as "unify member with each member of the list"
            var query = member <= list.Member & !(member <= list2.Member);

            // Run query and display all results
            int count = 0;

            foreach (var result in query)
            {
                Console.WriteLine($"member = {member}");
                count++;
            }

            Console.WriteLine($"Result count: {count}");
        }

        private static void SimpleDCG()
        {
            DisplayHeader("Simple DCG");

            // Create a list of phrase options
            var programmingLanguage = (Phrase)"Prolog" ^ "C#" ^ "Lisp" ^ "C++" ^ "Java";

            // Build a phrase from combined parts
            var sentence = "My favourite programming language is " + programmingLanguage + " - what's yours?";

            // Create an unbound character list
            var sentenceText = new Var<VarList<char>>();

            // Create query as "render the 'sentence' phrase to sentenceText"
            var query = sentence.BuildQuery(sentenceText);

            // Run query and display all results
            int count = 0;

            foreach (var result in query)
            {
                Console.WriteLine($"sentenceText = {sentenceText.AsString()}");
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
