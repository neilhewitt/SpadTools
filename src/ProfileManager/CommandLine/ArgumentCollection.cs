namespace ProfileManager
{
    public class ArgumentCollection : List<Argument>
    {
        new public Argument this[int index]
        {
            get
            {
                return this.ElementAtOrDefault(index);
            }
        }

        public Argument this[string argumentNameOrShortName]
        {
            get
            {
                return this.FirstOrDefault(a => a.Name == argumentNameOrShortName);
            }
        }

        public Argument this[(string Name, string ShortName) argument]
        {
            get
            {
                return this.FirstOrDefault(a => a.Name == argument.Name || a.Name == argument.ShortName);
            }
        }

        public void Add(string name, string value)
        {
            Add(new Argument(name, value));
        }

        public bool HasArguments => Count > 0;

        public ArgumentCollection(ArgumentCollection arguments)
            : base(arguments)
        {
        }

        public ArgumentCollection()
        {
        }
    }
}
