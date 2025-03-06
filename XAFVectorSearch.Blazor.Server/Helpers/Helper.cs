namespace XAFVectorSearch.Blazor.Server.Helpers;

public static class Helper
{
    public static string GetContentType(string file)
    {
        var fileExtension = Path.GetExtension(file).ToLowerInvariant();

        return fileExtension switch
        {
            ".pdf" => "application/pdf",
            ".txt" => "text/plain",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"  // Default to a binary content type if unknown
        };
    }
}
