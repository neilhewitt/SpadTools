using System.Net.Http.Headers;

namespace ProfileManager
{
    public class Argument
    {
        public string Name { get; init; }
        public string Value { get; init; }
        public Argument Next { get; internal set; }
        public Argument Previous { get; internal set; }

        public IEnumerable<Argument> FollowingArguments
        {
            get
            {
                Argument current = Next;
                while (current != null)
                {
                    yield return current;
                    current = current.Next;
                }
            }
        }

        public bool Matches(string argumentName, string shortArgumentName)
        {
            return Name == argumentName || Name == shortArgumentName;
        }

        override public string ToString()
        {
            return $"{Name}{(Value is not null ? ":" : "")}{Value}";
        }

        public Argument(string name, string value)
        {
            Name = name;
            Value = value ?? name;
        }
    }
}
