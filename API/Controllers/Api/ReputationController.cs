using Microsoft.AspNetCore.Mvc;
using SecurityAssessmentAPI.DTOs;
using SecurityAssessmentAPI.Services;

namespace SecurityAssessmentAPI.Controllers.Api
{
    [ApiController]
    [Route("api/reputation")]
    public class ReputationController : ControllerBase
    {
        private readonly IReputationCheckingService _reputationCheckingService;
        private readonly ILogger<ReputationController> _logger;

        public ReputationController(IReputationCheckingService reputationCheckingService, ILogger<ReputationController> logger)
        {
            _reputationCheckingService = reputationCheckingService;
            _logger = logger;
        }

        [HttpPost("check")]
        public async Task<IActionResult> CheckReputation([FromBody] ReputationCheckRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid reputation check request: {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))));
                return BadRequest(ModelState);
            }

            try
            {
                _logger.LogInformation("Reputation check request received: {Domain}", request.Domain);

                var result = await _reputationCheckingService.CheckReputationAsync(request.Domain, cancellationToken);

                _logger.LogInformation("Reputation check response sent: Domain={Domain}, Status={Status}, Score={Score}",
                    result.Domain, result.Status, result.OverallScore);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during reputation check: {Domain}", request.Domain);
                return StatusCode(500, new { message = "Reputation check could not be performed", error = ex.Message });
            }
        }

        [HttpGet("check/{domain}")]
        public async Task<IActionResult> GetReputationCheck(string domain, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                _logger.LogWarning("Invalid domain parameter: {Domain}", domain);
                return BadRequest("Domain is required");
            }

            try
            {
                _logger.LogInformation("Reputation check GET request received: {Domain}", domain);

                var result = await _reputationCheckingService.CheckReputationAsync(domain, cancellationToken);

                _logger.LogInformation("Reputation check GET response sent: Domain={Domain}, Status={Status}, Score={Score}",
                    result.Domain, result.Status, result.OverallScore);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during reputation check GET: {Domain}", domain);
                return StatusCode(500, new { message = "Reputation check could not be performed", error = ex.Message });
            }
        }
    }
}
