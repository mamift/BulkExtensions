namespace UnitTests.NetStandard.Model
{
    public class CompositeKeyEntity
    {
        public int FirstKeyEntityId { get; set; }
        public int SecondKeyEntityId { get; set; }

        public virtual FirstKeyEntity FirstKeyEntity { get; set; }
        public virtual SecondKeyEntity SecondKeyEntity { get; set; }

        public string Name { get; set; }
    }
}