namespace XAFVectorSearch.Blazor.Server.Controllers;

using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using XAFVectorSearch.Blazor.Server.Services;
using XAFVectorSearch.Module.Models;

[Route("api/[controller]")]
[ApiController]
public class VectorAPIController(IObjectSpaceFactory objectSpaceFactory) : ControllerBase
{

    readonly IObjectSpaceFactory objectSpaceFactory = objectSpaceFactory;

    [HttpGet]
    public async Task<IResult> Get(VectorSearchService vectorSearchService)
    {
        var documents = await vectorSearchService.GetDocumentsAsync();
        return TypedResults.Ok(documents);
    }

    ///api/documents/{documentId}/chunks/{documentChunkId}
    [HttpGet("{documentId:guid}/chunks")]
    public async Task<IResult> Get(Guid documentId, VectorSearchService vectorSearchService)
    {
        var documents = await vectorSearchService.GetDocumentChunksAsync(documentId);
        return TypedResults.Ok(documents);
    }

    [HttpGet("{documentId:guid}/chunks/{documentChunkId:guid}")]
    public async Task<IResult> Get(Guid documentId, Guid documentChunkId, VectorSearchService vectorSearchService)
    {
        var chunk = await vectorSearchService.GetDocumentChunkEmbeddingAsync(documentId, documentChunkId);
        if (chunk is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(chunk);
    }


    [HttpPost]
    public async Task<IResult> Post(VectorSearchService vectorSearchService, Guid? documentId = null)
    {

        if (!documentId.HasValue)
        {
            return TypedResults.BadRequest("Document ID is required.");
        }

        using IObjectSpace newObjectSpace = objectSpaceFactory.CreateObjectSpace<Module.BusinessObjects.Documents>();
        Module.BusinessObjects.Documents  documents = newObjectSpace.GetObjectByKey<Module.BusinessObjects.Documents>(documentId.Value);
        if (documents == null)
        {
            return TypedResults.NotFound($"Document with ID {documentId.Value} not found.");
            
        }

        documentId = await vectorSearchService.ImportAsync(documentId);

        return TypedResults.Ok(new UploadDocumentResponse(documentId.Value));
    }

    

    [HttpDelete("{documentId:guid}")]
    public IResult Delete(Guid documentId, VectorSearchService vectorSearchService)
    {
        vectorSearchService.DeleteDocument(documentId);
        return TypedResults.NoContent();
    }

    [HttpPost("ask")]
    public async Task<IResult> PostAsk(Question question, VectorSearchService vectorSearchService, bool reformulate = true)
    {
        var response = await vectorSearchService.AskQuestionAsync(question, reformulate);
        return TypedResults.Ok(response);
    }


    [HttpPost("ask-streaming")]
    public IAsyncEnumerable<Response> PostAskStreaming(Question question, VectorSearchService vectorSearchService, bool reformulate = true)
    {
        async IAsyncEnumerable<Response> Stream()
        {
            // Requests a streaming response.
            var responseStream = vectorSearchService.AskStreamingAsync(question, reformulate);

            await foreach (var delta in responseStream)
            {
                yield return delta;
            }
        }

        return Stream();
    }

}
