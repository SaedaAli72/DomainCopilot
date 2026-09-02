

using DomainCopilot.Application.Agents.Contracts;
using DomainCopilot.Application.Agents.Interfaces;

namespace DomainCopilot.Application.Agents
{
    public class GovernmentServiceOrchestrator:IGovernmentServiceOrchestrator
    {
        private readonly IEligibilityIdentifierAgent _eligibilityAgent;
        private readonly IProcedureResolverAgent _procedureAgent;
        private readonly IResponseDrafterAgent _responseDrafterAgent;

        public GovernmentServiceOrchestrator(
            IEligibilityIdentifierAgent eligibilityAgent,
            IProcedureResolverAgent procedureAgent,
            IResponseDrafterAgent responseDrafterAgent)
        {
            _eligibilityAgent = eligibilityAgent;
            _procedureAgent = procedureAgent;
            _responseDrafterAgent = responseDrafterAgent;
        }

        public async Task<DraftedResponse?> ProcessAsync(string citizenSituation, string serviceType, Guid tenantId, CancellationToken cancellationToken = default)
        {
            // Step 1: check eligibility
            var eligibility = await _eligibilityAgent.AnalyzeAsync(citizenSituation, tenantId, cancellationToken);

            // Stop immediately if human escalation is required — do not guess further
            if (eligibility.RequiresEscalation)
            {
                return null;
            }

            // Step 2: resolve procedure (documents, timeline, fees)
            var procedure = await _procedureAgent.ResolveAsync(serviceType, tenantId, cancellationToken);

            // Step 3: draft the final response, combining both results
            var draftedResponse = await _responseDrafterAgent.DraftAsync(eligibility, procedure, cancellationToken);

            return draftedResponse;
        }
    }
}

