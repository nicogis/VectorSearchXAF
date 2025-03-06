namespace XAFVectorSearch.Blazor.Server.ContentDecoders;

public interface IContentDecoder
{
    Task<string> DecodeAsync(Stream stream, string contentType);
}
