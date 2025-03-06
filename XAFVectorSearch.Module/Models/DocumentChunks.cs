
#nullable enable
using DevExpress.Persistent.BaseImpl.EF;

namespace XAFVectorSearch.Module.BusinessObjects
{

    public partial class DocumentChunks
    {
    public virtual Guid ID { get; set; }

    public virtual Guid DocumentId { get; set; }

    public virtual int Index { get; set; }

    public virtual required string Content { get; set; }

    public virtual required float[] Embedding { get; set; }

    public virtual Documents Document { get; set; } = null!;
    }
}


namespace XAFVectorSearch.Module.Models
{
    public record class DocumentChunks(Guid Id, int Index, string Content, float[]? Embedding = null);
}