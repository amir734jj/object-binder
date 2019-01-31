namespace core.Tests.Models
{
    public interface ICommon
    {
        string NameC { get; set; }

        bool AliveC { get; set; }
        
        Entity SourceRef { get; }
    }
}