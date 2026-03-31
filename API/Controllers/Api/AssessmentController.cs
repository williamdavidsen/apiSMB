using Microsoft.AspNetCore.Mvc;
using SecurityAssessmentAPI.DTOs;
using SecurityAssessmentAPI.Services;

namespace SecurityAssessmentAPI.Controllers.Api
{
    [ApiController]
    [Route("api/assessment")]
    public class AssessmentController : ControllerBase
    {
        private readonly IAssessmentCheckingService _assessmentCheckingService;
        private readonly ILogger<AssessmentController> _logger;

        public AssessmentController(IAssessmentCheckingService assessmentCheckingService, ILogger<AssessmentController> logger)
        {
            _assessmentCheckingService = assessmentCheckingService;
            _logger = logger;
        }

        [HttpPost("check")]
        public async Task<IActionResult> CheckAssessment([FromBody] AssessmentCheckRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid assessment check request: {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))));
                return BadRequest(ModelState);
            }

            try
            {
                _logger.LogInformation("Assessment check request received: {Domain}", request.Domain);

                var result = await _assessmentCheckingService.CheckAssessmentAsync(request.Domain, cancellationToken);

                _logger.LogInformation("Assessment check response sent: Domain={Domain}, Status={Status}, Score={Score}",
                    result.Domain, result.Status, result.OverallScore);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during assessment check: {Domain}", request.Domain);
                return StatusCode(500, new { message = "Assessment check could not be performed", error = ex.Message });
            }
        }

        [HttpGet("check/{domain}")]
        public async Task<IActionResult> GetAssessmentCheck(string domain, CancellationToken cancellationToken)
        {
            // Keep a route-based variant for quick manual testing and batch execution.
            if (string.IsNullOrWhiteSpace(domain))
            {
                _logger.LogWarning("Invalid domain parameter: {Domain}", domain);
                return BadRequest("Domain is required");
            }

            try
            {
                _logger.LogInformation("Assessment check GET request received: {Domain}", domain);

                var result = await _assessmentCheckingService.CheckAssessmentAsync(domain, cancellationToken);

                _logger.LogInformation("Assessment check GET response sent: Domain={Domain}, Status={Status}, Score={Score}",
                    result.Domain, result.Status, result.OverallScore);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during assessment check GET: {Domain}", domain);
                return StatusCode(500, new { message = "Assessment check could not be performed", error = ex.Message });
            }
        }
    }
}
