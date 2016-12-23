using System;
using System.Collections.Generic;
using System.Linq;

namespace Keeper.BacktraQ
{
    class Program
    {
        static void Main(string[] args)
        {
            var culture = new Culture();

            var characterList = new CharacterList();

            //var playerCharacter = characterList.New();

            Console.ReadLine();
        }
    }

    public class CharacterList
    {
        private Var<VarList<Character>> list = VarList.Create<Character>();
        private Culture culture = new Culture();

        //public Query New(Var<Character> character)
        //{
        //    var newCharacter = new Character(this.culture);

        //    return 
        //}
    }
}
