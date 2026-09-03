using DomainCopilot.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DomainCopilot.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApprovalController : ControllerBase
    {
        private readonly ICitizenRequestRepository _citizenRequestRepository;

        public ApprovalController(ICitizenRequestRepository citizenRequestRepository)
        {
            _citizenRequestRepository = citizenRequestRepository;
        }

        // GET /api/Approval/pending — list all requests waiting for officer review
        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            var tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var pending = await _citizenRequestRepository.GetPendingApprovalAsync(tenantId);
            return Ok(pending);
        }

        // POST /api/Approval/{id}/approve — officer approves the drafted response
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(Guid id)
        {
            var request = await _citizenRequestRepository.GetByIdAsync(id);
            if (request is null) return NotFound();

            request.Approve();
            await _citizenRequestRepository.UpdateAsync(request);

            return Ok(new { status = "approved", request.Id, request.DraftedResponseText });
        }

        // POST /api/Approval/{id}/reject — officer rejects the drafted response
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> Reject(Guid id)
        {
            var request = await _citizenRequestRepository.GetByIdAsync(id);
            if (request is null) return NotFound();

            request.Reject();
            await _citizenRequestRepository.UpdateAsync(request);

            return Ok(new { status = "rejected", request.Id });
        }
    }
}
