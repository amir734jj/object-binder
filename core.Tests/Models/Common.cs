namespace core.Tests.Models
{
    public class Common : ICommon
    {
        public virtual string NameC { get; set; }
        public virtual bool AliveC { get; set; }
        
        public string ExtraProperty { get; set; }
    }
}