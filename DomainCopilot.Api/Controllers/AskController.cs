using DomainCopilot.Application.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DomainCopilot.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AskController : ControllerBase
    {
        private readonly AskQuestionUseCase _askQuestionUseCase;

        public AskController(AskQuestionUseCase askQuestionUseCase)
        {
            _askQuestionUseCase = askQuestionUseCase;
        }

        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] string question)
        {
            var tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111"); // placeholder — real tenant resolution comes later
            var answer = await _askQuestionUseCase.ExecuteAsync(question, tenantId);
            return Ok(answer);
        }
    }
}
