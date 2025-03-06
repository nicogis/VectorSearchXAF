
#nullable disable
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using System.Collections.ObjectModel;

namespace XAFVectorSearch.Module.BusinessObjects
{
    public partial class Documents
    {
        public virtual Guid ID { get; set; }


        [ExpandObjectMembers(ExpandObjectMembers.Never)]
        [FileTypeFilter("File docx,txt", 1, "*.txt", "*.docx")]
        [FileTypeFilter("File Pdf", 2, "*.pdf")]
        public virtual FileData File { get; set; }

        public virtual IList<DocumentChunks> DocumentChunks { get; set; } = new ObservableCollection<DocumentChunks>();
    }
}


namespace XAFVectorSearch.Module.Models
{
    public record class Documents(Guid Id, string Name, int ChunkCount);
}