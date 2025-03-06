using System.Text;
using DevExpress.XtraRichEdit;
using DevExpress.XtraRichEdit.API.Native;

namespace XAFVectorSearch.Blazor.Server.ContentDecoders;

public class DocxContentDecoder : IContentDecoder
{
    public Task<string> DecodeAsync(Stream stream, string contentType)
    {
        using var wordProcessor = new RichEditDocumentServer();
        wordProcessor.LoadDocument(stream, DocumentFormat.OpenXml);

        var document = wordProcessor.Document;
        var content = new StringBuilder();

        foreach (Paragraph paragraph in document.Paragraphs)
        {
            content.AppendLine(document.GetText(paragraph.Range));
        }

        return Task.FromResult(content.ToString());
    }
}
