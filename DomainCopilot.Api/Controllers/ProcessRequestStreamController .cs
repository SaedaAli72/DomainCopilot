using DomainCopilot.Application.Agents.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace DomainCopilot.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProcessRequestStreamController : ControllerBase
    {
        private readonly IEligibilityIdentifierAgent _eligibilityAgent;
        private readonly IProcedureResolverAgent _procedureAgent;
        private readonly IResponseDrafterAgent _responseDrafterAgent;

        public ProcessRequestStreamController(
            IEligibilityIdentifierAgent eligibilityAgent,
            IProcedureResolverAgent procedureAgent,
            IResponseDrafterAgent responseDrafterAgent)
        {
            _eligibilityAgent = eligibilityAgent;
            _procedureAgent = procedureAgent;
            _responseDrafterAgent = responseDrafterAgent;
        }

        [HttpGet("stream")]
        public async Task Stream(string citizenSituation, string serviceType, CancellationToken cancellationToken)
        {
            Response.Headers.Append("Content-Type", "text/event-stream");
            var tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

            await SendEvent("Eligibility check started...");
            var eligibility = await _eligibilityAgent.AnalyzeAsync(citizenSituation, tenantId, cancellationToken);
            await SendEvent($"Eligibility: {(eligibility.IsEligible ? "Eligible" : "Not eligible")}");

            if (eligibility.RequiresEscalation)
            {
                await SendEvent("Escalated — human review required.");
                return;
            }

            await SendEvent("Resolving procedure...");
            var procedure = await _procedureAgent.ResolveAsync(serviceType, tenantId, cancellationToken);
            await SendEvent("Procedure resolved.");

            await SendEvent("Drafting response...");
            var draft = await _responseDrafterAgent.DraftAsync(eligibility, procedure, cancellationToken);
            await SendEvent("Draft ready. Awaiting officer approval.");

            async Task SendEvent(string message)
            {
                var json = JsonSerializer.Serialize(new { message });
                await Response.WriteAsync($"data: {json}\n\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }
        }
    }
}
