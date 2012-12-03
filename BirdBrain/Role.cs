namespace BirdBrain
{
    public class Role
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public Role(string name)
        {
            Id = "roles/" + name;
            Name = name;
        }
    }
}
