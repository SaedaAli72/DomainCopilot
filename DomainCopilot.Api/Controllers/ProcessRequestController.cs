using DomainCopilot.Application.Agents.Interfaces;
using DomainCopilot.Application.Interfaces;
using DomainCopilot.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DomainCopilot.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProcessRequestController : ControllerBase
    {
        private readonly IGovernmentServiceOrchestrator _orchestrator;
        private readonly ICitizenRequestRepository _citizenRequestRepository;

        public ProcessRequestController(
            IGovernmentServiceOrchestrator orchestrator,
            ICitizenRequestRepository citizenRequestRepository)
        {
            _orchestrator = orchestrator;
            _citizenRequestRepository = citizenRequestRepository;
        }

        public record ProcessRequest(string CitizenName, string CitizenSituation, string ServiceType);

        [HttpPost]
        public async Task<IActionResult> Process([FromBody] ProcessRequest request)
        {
            var tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

            var citizenRequest = new CitizenRequest(tenantId, request.CitizenName, request.ServiceType);
            await _citizenRequestRepository.AddAsync(citizenRequest);

            var draftedResponse = await _orchestrator.ProcessAsync(request.CitizenSituation, request.ServiceType, tenantId);

            if (draftedResponse is null)
            {
                citizenRequest.Escalate();
                await _citizenRequestRepository.UpdateAsync(citizenRequest);
                return Ok(new { status = "escalated", requestId = citizenRequest.Id });
            }

            citizenRequest.AttachDraftedResponse(
                draftedResponse.ResponseText,
                draftedResponse.Eligibility.Reason,
                string.Join(", ", draftedResponse.Procedure.RequiredDocuments));

            await _citizenRequestRepository.UpdateAsync(citizenRequest);

            return Ok(new { status = "awaiting_approval", requestId = citizenRequest.Id, draftedResponse.ResponseText });
        }
    }
}
