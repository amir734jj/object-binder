namespace core.Tests.Models
{
    public interface ICommon
    {
        string NameC { get; set; }

        bool AliveC { get; set; }
    }
    
    public class Hello : ICommon {
        public virtual string NameC { get; set; }
        public virtual bool AliveC { get; set; }
    }
    
    public class HelloX : Hello {
        public override string NameC { get; set; }
        public override bool AliveC { get; set; }
    }
}