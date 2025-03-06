#nullable enable

using DevExpress.Data.Linq.Helpers;
using DevExpress.ExpressApp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Text;
using System.Data;
using XAFVectorSearch.Blazor.Server.ContentDecoders;
using XAFVectorSearch.Blazor.Server.Settings;
using XAFVectorSearch.Module.Models;



namespace XAFVectorSearch.Blazor.Server.Services;

#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
public class VectorSearchService(IServiceProvider serviceProvider, IObjectSpaceFactory objectSpaceFactory,  ITextEmbeddingGenerationService textEmbeddingGenerationService, ChatService chatService, TokenizerService tokenizerService, IOptions<AppSettings> appSettingsOptions, ILogger<VectorSearchService> logger)
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
{
    private readonly AppSettings appSettings = appSettingsOptions.Value;

    public Task<Guid> ImportAsync(Guid? documentId)
    {
        
        using IObjectSpace newObjectSpace = objectSpaceFactory.CreateObjectSpace<Module.BusinessObjects.Documents>();
        Module.BusinessObjects.Documents documents = newObjectSpace.GetObjectByKey<Module.BusinessObjects.Documents>(documentId!.Value);
        
        using var stream = new MemoryStream();
        documents.File.SaveToStream(stream);

        return ImportAsync(stream, Helpers.Helper.GetContentType(documents.File.FileName), documentId);

    }
    
    public async Task<Guid> ImportAsync(Stream stream, string contentType, Guid? documentId)
    {
        // Extract the contents of the file.
        var decoder = serviceProvider.GetKeyedService<IContentDecoder>(contentType) ?? throw new NotSupportedException($"Content type '{contentType}' is not supported.");
        var content = await decoder.DecodeAsync(stream, contentType);

        using IObjectSpace objectSpace = objectSpaceFactory.CreateObjectSpace<Module.BusinessObjects.Documents>();

        var document = objectSpace.GetObjectByKey<Module.BusinessObjects.Documents>(documentId.GetValueOrDefault());

        

        // Split the content into chunks and generate the embeddings for each one.
#pragma warning disable SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        var lines = TextChunker.SplitPlainTextLines(content, appSettings.MaxTokensPerLine, tokenizerService.CountTokens);

        var paragraphs = TextChunker.SplitPlainTextParagraphs(lines, appSettings.MaxTokensPerParagraph, appSettings.OverlapTokens, tokenCounter: tokenizerService.CountTokens);
#pragma warning restore SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        var embeddings = await textEmbeddingGenerationService.GenerateEmbeddingsAsync(paragraphs);

        // Save the document chunks and the corresponding embedding in the database.
        foreach (var (index, paragraph) in paragraphs.Index())
        {
            logger.LogInformation("Storing a paragraph of {TokenCount} tokens.", tokenizerService.CountTokens(paragraph));

            var documentChunk = objectSpace.CreateObject<Module.BusinessObjects.DocumentChunks>();
            documentChunk.Index = index;
            documentChunk.Content = paragraph!;
            documentChunk.Embedding = embeddings[index].ToArray();
            document.DocumentChunks.Add(documentChunk);
        }

        

        objectSpace.CommitChanges();

        return document.ID;
    }

    public async Task<IEnumerable<Module.Models.Documents>> GetDocumentsAsync()
    {
        using IObjectSpace objectSpace = objectSpaceFactory.CreateObjectSpace<Module.BusinessObjects.Documents>();

        var documents = await objectSpace.GetObjectsQuery<Module.BusinessObjects.Documents>().OrderBy(d => d.File.FileName)
            .Select(d => new Module.Models.Documents(d.ID, d.File.FileName, d.DocumentChunks.Count))
            .ToListAsync();

        return documents;
    }

    public async Task<IEnumerable<Module.Models.DocumentChunks>> GetDocumentChunksAsync(Guid documentId)
    {
        using IObjectSpace objectSpace = objectSpaceFactory.CreateObjectSpace<Module.BusinessObjects.DocumentChunks>();

        var documentChunks = await objectSpace.GetObjectsQuery<Module.BusinessObjects.DocumentChunks>().Where(c => c.DocumentId == documentId).OrderBy(c => c.Index)
            .Select(c => new Module.Models.DocumentChunks(c.ID, c.Index, c.Content, null))
            .ToListAsync();

        return documentChunks;
    }

    public async Task<Module.Models.DocumentChunks?> GetDocumentChunkEmbeddingAsync(Guid documentId, Guid documentChunkId)
    {
        using IObjectSpace objectSpace = objectSpaceFactory.CreateObjectSpace<Module.BusinessObjects.DocumentChunks>();

        var documentChunk = await objectSpace.GetObjectsQuery<Module.BusinessObjects.DocumentChunks>().Where(c => c.ID == documentChunkId && c.DocumentId == documentId)
            .Select(c => new Module.Models.DocumentChunks(c.ID, c.Index, c.Content, c.Embedding))
            .FirstOrDefaultAsync();

        return documentChunk;
    }

    public void DeleteDocument(Guid documentId)
    {
        using IObjectSpace objectSpace = objectSpaceFactory.CreateObjectSpace<Module.BusinessObjects.Documents>();
        var doc = objectSpace.GetObjectByKey<Module.BusinessObjects.Documents>(documentId);
        objectSpace.Delete(doc);
        objectSpace.CommitChanges();    
    }

    public async Task<Response> AskQuestionAsync(Question question, bool reformulate = true)
    {
        var (reformulatedQuestion, chunks) = await CreateContextAsync(question, reformulate);

        var answer = await chatService.AskQuestionAsync(question.ConversationId, chunks, reformulatedQuestion);
        return new Response(reformulatedQuestion, answer);
    }

    public async IAsyncEnumerable<Response> AskStreamingAsync(Question question, bool reformulate = true)
    {
        var (reformulatedQuestion, chunks) = await CreateContextAsync(question, reformulate);

        var answerStream = chatService.AskStreamingAsync(question.ConversationId, chunks, reformulatedQuestion);

        // The first message contains the original question.
        yield return new Response(reformulatedQuestion, null, StreamState.Start);

        // Return each token as a partial response.
        await foreach (var token in answerStream)
        {
            yield return new Response(null, token, StreamState.Append);
        }

        // The last message tells the client that the stream has ended.
        yield return new Response(null, null, StreamState.End);
    }

    private async Task<(string Question, IEnumerable<string> Chunks)> CreateContextAsync(Question question, bool reformulate = true)
    {
        // Reformulate the following question taking into account the context of the chat to perform keyword search and embeddings:
        var reformulatedQuestion = reformulate ? await chatService.CreateQuestionAsync(question.ConversationId, question.Text) : question.Text;

        // Perform Vector Search on SQL Database.
        var questionEmbedding = await textEmbeddingGenerationService.GenerateEmbeddingAsync(reformulatedQuestion);

        using IObjectSpace objectSpace = objectSpaceFactory.CreateObjectSpace<Module.BusinessObjects.DocumentChunks>();

        var chunks = await objectSpace.GetObjectsQuery<Module.BusinessObjects.DocumentChunks>()
                    .OrderBy(c => EF.Functions.VectorDistance("cosine", c.Embedding, questionEmbedding.ToArray()))
                    .Select(c => c.Content)
                    .Take(appSettings.MaxRelevantChunks)
                    .ToListAsync();

        return (reformulatedQuestion, chunks);
    }
}