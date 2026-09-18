using DomainCopilot.Application.Interfaces;
using DomainCopilot.Domain.Enum;
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

        /// <summary>
        /// Get all pending approval requests
        /// </summary>
        [HttpGet("pending")]
        public async Task<IActionResult> GetPending(CancellationToken cancellationToken = default)
        {
            try
            {
                var tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
                Console.WriteLine($"[Approval/pending] Fetching requests for tenantId: {tenantId}");

                var pending = await _citizenRequestRepository.GetPendingApprovalAsync(tenantId, cancellationToken);

                Console.WriteLine($"[Approval/pending] Found {pending.Count} requests");
                foreach (var req in pending)
                {
                    Console.WriteLine($"  - ID: {req.Id}, Name: {req.CitizenName}, Status: {req.Status}");
                }

                var result = pending.Select(r => new
                {
                    id = r.Id.ToString(),
                    citizenName = r.CitizenName ?? "N/A",
                    serviceType = r.ServiceType ?? "N/A",
                    status = r.Status.ToString(),
                    createdAt = r.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                    draftedResponseText = r.DraftedResponseText ?? "No response yet",
                    eligibilityReason = r.EligibilityReason ?? "N/A",
                    requiredDocumentsSummary = r.RequiredDocumentsSummary ?? "N/A"
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetPending: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        /// <summary>
        /// Get a specific request by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = await _citizenRequestRepository.GetByIdAsync(id, cancellationToken);

                if (request is null)
                    return NotFound(new { error = "Request not found" });

                return Ok(new
                {
                    id = request.Id.ToString(),
                    citizenName = request.CitizenName ?? "N/A",
                    serviceType = request.ServiceType ?? "N/A",
                    status = request.Status.ToString(),
                    createdAt = request.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                    draftedResponseText = request.DraftedResponseText ?? "No response yet",
                    eligibilityReason = request.EligibilityReason ?? "N/A",
                    requiredDocumentsSummary = request.RequiredDocumentsSummary ?? "N/A"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetById: {ex.Message}");
                return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        /// <summary>
        /// Officer approves the drafted response
        /// </summary>
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = await _citizenRequestRepository.GetByIdAsync(id, cancellationToken);

                if (request is null)
                    return NotFound(new { error = "Request not found" });

                request.Approve();
                await _citizenRequestRepository.UpdateAsync(request, cancellationToken);

                return Ok(new
                {
                    message = "Request approved successfully",
                    status = request.Status.ToString(),
                    requestId = request.Id.ToString()
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Approve: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Officer rejects the drafted response
        /// </summary>
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> Reject(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = await _citizenRequestRepository.GetByIdAsync(id, cancellationToken);

                if (request is null)
                    return NotFound(new { error = "Request not found" });

                request.Reject();
                await _citizenRequestRepository.UpdateAsync(request, cancellationToken);

                return Ok(new
                {
                    message = "Request rejected successfully",
                    status = request.Status.ToString(),
                    requestId = request.Id.ToString()
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Reject: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get all requests with optional status filter
        /// </summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetAll([FromQuery] string? status = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
                var pending = await _citizenRequestRepository.GetPendingApprovalAsync(tenantId, cancellationToken);

                // Filter by status if provided
                if (!string.IsNullOrEmpty(status))
                {
                    if (Enum.TryParse<RequestStatus>(status, true, out var statusEnum))
                    {
                        pending = pending.Where(r => r.Status == statusEnum).ToList();
                    }
                }

                var result = pending.Select(r => new
                {
                    id = r.Id.ToString(),
                    citizenName = r.CitizenName ?? "N/A",
                    serviceType = r.ServiceType ?? "N/A",
                    status = r.Status.ToString(),
                    createdAt = r.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                    draftedResponseText = r.DraftedResponseText ?? "No response yet",
                    eligibilityReason = r.EligibilityReason ?? "N/A",
                    requiredDocumentsSummary = r.RequiredDocumentsSummary ?? "N/A"
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAll: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}