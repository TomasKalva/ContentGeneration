namespace Assets.Util
{
    public class Annotated<T>
    {
        public string Name { get; }
        public string Description { get; }
        public T Item { get; }

        public Annotated(string name, string description, T item)
        {
            Name = name;
            Description = description;
            this.Item = item;
        }
    }
}
