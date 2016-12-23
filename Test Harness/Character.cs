namespace Keeper.BacktraQ
{
    public class Character
    {
        public Character(Culture culture)
        {
            this.Culture = culture;
            this.Gender = Generated.Random(BacktraQ.Gender.Male, BacktraQ.Gender.Female);
            this.Name = new Generated<string>(x => this.Culture.GenerateName(x, this.Gender.Value));
        }

        public Culture Culture
        {
            get;
            private set;
        }

        public Generated<string> Name
        {
            get;
            private set;
        }

        public Generated<Gender> Gender
        {
            get;
            private set;
        }
    }
}

namespace Keeper.BacktraQ
{
    public enum Gender
    {
        Male,
        Female
    }
}