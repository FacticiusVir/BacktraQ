using System;

namespace Keeper.BacktraQ
{
    public class Culture
    {
        public Query GenerateName(Var<string> name, Gender gender)
        {
            var forename = Phrase.SwitchPhrase(gender, Phrase.SwitchCase(Gender.Male, Phrase.RandomPhrase("John", "James", "William", "Tony")),
                                                Phrase.SwitchCase(Gender.Female, Phrase.RandomPhrase("Jane", "Emma", "Susan", "Sarah")));

            var surname = Phrase.RandomPhrase("Smith", "Jones");

            return Phrase.ChainPhrase(forename, " ", surname)
                            .AsString(name);
        }
    }
}