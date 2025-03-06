using System.Text;
using DevExpress.Pdf;

namespace XAFVectorSearch.Blazor.Server.ContentDecoders;

public class PdfContentDecoder : IContentDecoder
{
    public Task<string> DecodeAsync(Stream stream, string contentType)
    {
        var content = new StringBuilder();

        
        using var pdfDocument = new PdfDocumentProcessor();
        pdfDocument.LoadDocument(stream);

        for (int i = 0; i < pdfDocument.Document.Pages.Count; i++)
        {
            var pageContent = pdfDocument.GetPageText(i+1, new PdfTextExtractionOptions { ClipToCropBox = false }) ?? string.Empty;
            content.AppendLine(pageContent);
        }

        return Task.FromResult(content.ToString());
    }
}



