using DomainCopilot.Application.Agents.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DomainCopilot.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProcessRequestController : ControllerBase
    {
        private readonly IGovernmentServiceOrchestrator _orchestrator;

        public ProcessRequestController(IGovernmentServiceOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        public record ProcessRequest(string CitizenSituation, string ServiceType);

        [HttpPost]
        public async Task<IActionResult> Process([FromBody] ProcessRequest request)
        {
            var tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

            var result = await _orchestrator.ProcessAsync(request.CitizenSituation, request.ServiceType, tenantId);

            if (result is null)
            {
                return Ok(new { status = "escalated", message = "This request requires human review." });
            }

            return Ok(result);
        }
    }
}
