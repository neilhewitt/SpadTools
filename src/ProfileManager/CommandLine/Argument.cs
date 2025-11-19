using System.Net.Http.Headers;

namespace ProfileManager
{
    public class Argument
    {
        public string Name { get; init; }
        public string Value { get; init; }

        public bool IsValueOnly => Name == Value;
        public bool IsNamed => Name != Value;

        public string[] GetValues(params char[] separators)
        {
            if (separators.Length == 0) separators = new [] { ',' };
            return (Value ?? "").Split(separators, StringSplitOptions.RemoveEmptyEntries);
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

        public Argument(string value)
        {
            Name = value;
            Value = value;
        }

        public static implicit operator string(Argument argument)
        {
            return argument?.Value ?? null;
        }

        public static implicit operator Argument(string value)
        {
            return new Argument(value);
        }

        public static implicit operator Argument((string name, string value) argument)
        {
            return new Argument(argument.name, argument.value);
        }
    }
}
