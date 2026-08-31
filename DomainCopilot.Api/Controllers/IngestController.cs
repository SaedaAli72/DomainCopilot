using DomainCopilot.Application.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DomainCopilot.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IngestController : ControllerBase
    {
        private readonly IngestDocumentUseCase _ingestDocumentUseCase;

        public IngestController(IngestDocumentUseCase ingestDocumentUseCase)
        {
            _ingestDocumentUseCase = ingestDocumentUseCase;
        }

        public record IngestRequest(string FileName, string RawText);

        [HttpPost]
        public async Task<IActionResult> Ingest([FromBody] IngestRequest request)
        {
            var tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111"); // placeholder — real tenant resolution comes later
            var documentId = await _ingestDocumentUseCase.ExecuteAsync(tenantId, request.FileName, request.RawText);
            return Ok(new { documentId });
        }
    }
}
