using Microsoft.AspNetCore.Mvc;
using SecurityAssessmentAPI.Services;

namespace SecurityAssessmentAPI.Controllers.Api
{
    [ApiController]
    [Route("api/pqc")]
    public class PqcController : ControllerBase
    {
        private readonly IPqcCheckingService _pqcCheckingService;
        private readonly ILogger<PqcController> _logger;

        public PqcController(IPqcCheckingService pqcCheckingService, ILogger<PqcController> logger)
        {
            _pqcCheckingService = pqcCheckingService;
            _logger = logger;
        }

        [HttpGet("check/{domain}")]
        public async Task<IActionResult> GetPqcCheck(string domain, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                _logger.LogWarning("Invalid PQC domain parameter: {Domain}", domain);
                return BadRequest("Domain is required");
            }

            try
            {
                _logger.LogInformation("PQC check GET request received: {Domain}", domain);

                var result = await _pqcCheckingService.CheckPqcAsync(domain, cancellationToken);

                _logger.LogInformation("PQC check GET response sent: Domain={Domain}, Status={Status}, Readiness={ReadinessLevel}",
                    result.Domain, result.Status, result.ReadinessLevel);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during PQC check GET: {Domain}", domain);
                return StatusCode(500, new { message = "PQC check could not be performed", error = ex.Message });
            }
        }
    }
}
