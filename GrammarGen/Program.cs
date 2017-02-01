using Keeper.BacktraQ;
using System;
using System.Linq;

using static GrammarGen.Program.Definiteness;
using static GrammarGen.Program.Tense;
using static Keeper.BacktraQ.Phrase;

namespace GrammarGen
{
    class Program
    {
        public enum Definiteness
        {
            Definite,
            Indefinite
        }

        public enum Person
        {
            First,
            Second,
            Third
        }

        public enum Tense
        {
            Objective,
            Subjective
        }

        static void Main(string[] args)
        {
            var tail = VarList<char>.EmptyList;
            var text = new Var<VarList<char>>();
            var output = new Var<VarList<char>>();

            foreach (var result in (SimpleSentence().BuildQuery(text, tail) & Capitalise(text, output))
                                    .AsEnumerable())
            {
                Console.WriteLine(output.AsString());
            }

            Console.ReadLine();
        }

        private static Query Capitalise(Var<VarList<char>> lower, Var<VarList<char>> upper)
        {
            var tail = new Var<VarList<char>>();
            var lowerHead = new Var<char>();
            var upperHead = new Var<char>();

            return lower.Unify(lowerHead, tail)
                        & upper.Unify(upperHead, tail)
                        & Capitalise(lowerHead, upperHead);
        }

        private static Query Capitalise(Var<char> lower, Var<char> upper)
        {
            return Query.Map(lower, upper, char.ToUpper, char.ToLower);
        }

        private static Phrase SimpleSentence()
        {
            var person = new Var<Person>();

            return NounPhrase(person, Subjective) + " " + VerbPhrase(person) + ".";
        }

        private static Phrase VerbPhrase(Var<Person> person = null)
        {
            return Verb(person) + " " + NounPhrase(tense: Objective);
        }

        private static Phrase NounPhrase(Var<Person> person = null, Var<Tense> tense = null)
        {
            return ((person <= Person.Third) + Determiner() + " " + Noun())
                        ^ Pronoun(person, tense);
        }

        private static Phrase Pronoun(Var<Person> person = null, Var<Tense> tense = null)
        {
            return SwitchPhrase(tense, SwitchCase(Subjective, SwitchPhrase(person, SwitchCase(Person.First, "I"),
                                                                                    SwitchCase(Person.Second, "you"),
                                                                                    SwitchCase(Person.Third, "he"),
                                                                                    SwitchCase(Person.Third, "she"),
                                                                                    SwitchCase(Person.Third, "it"))),
                                        SwitchCase(Objective, SwitchPhrase(person, SwitchCase(Person.First, "me"),
                                                                                    SwitchCase(Person.Second, "you"),
                                                                                    SwitchCase(Person.Third, "him"),
                                                                                    SwitchCase(Person.Third, "her"),
                                                                                    SwitchCase(Person.Third, "it"))));
        }

        private static Phrase Noun()
        {
            return (Phrase)"room" ^ "chair" ^ "table";
        }

        private static Phrase Verb(Var<Person> person = null)
        {
            return SwitchPhrase(person, SwitchCase(Person.First, "have"),
                                        SwitchCase(Person.Second, "have"),
                                        SwitchCase(Person.Third, "has"));
        }

        private static Phrase Determiner(Var<Definiteness> definiteness = null)
        {
            return SwitchPhrase(definiteness, SwitchCase(Indefinite, "a"),
                                                SwitchCase(Definite, "the"));
        }
    }
}
