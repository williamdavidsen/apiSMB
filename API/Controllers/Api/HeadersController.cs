using Microsoft.AspNetCore.Mvc;
using SecurityAssessmentAPI.DTOs;
using SecurityAssessmentAPI.Services;

namespace SecurityAssessmentAPI.Controllers.Api
{
    [ApiController]
    [Route("api/headers")]
    public class HeadersController : ControllerBase
    {
        private readonly IHeadersCheckingService _headersCheckingService;
        private readonly ILogger<HeadersController> _logger;

        public HeadersController(IHeadersCheckingService headersCheckingService, ILogger<HeadersController> logger)
        {
            _headersCheckingService = headersCheckingService;
            _logger = logger;
        }

        [HttpPost("check")]
        public async Task<IActionResult> CheckHeaders([FromBody] HeadersCheckRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid headers check request: {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))));
                return BadRequest(ModelState);
            }

            try
            {
                _logger.LogInformation("Headers check request received: {Domain}", request.Domain);

                var result = await _headersCheckingService.CheckHeadersAsync(request.Domain, cancellationToken);

                _logger.LogInformation("Headers check response sent: Domain={Domain}, Status={Status}, Score={Score}",
                    result.Domain, result.Status, result.OverallScore);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during headers check: {Domain}", request.Domain);
                return StatusCode(500, new { message = "Headers check could not be performed", error = ex.Message });
            }
        }

        [HttpGet("check/{domain}")]
        public async Task<IActionResult> GetHeadersCheck(string domain, CancellationToken cancellationToken)
        {
            // Support lightweight GET checks for Swagger, browser tests, and scripted demos.
            if (string.IsNullOrWhiteSpace(domain))
            {
                _logger.LogWarning("Invalid domain parameter: {Domain}", domain);
                return BadRequest("Domain is required");
            }

            try
            {
                _logger.LogInformation("Headers check GET request received: {Domain}", domain);

                var result = await _headersCheckingService.CheckHeadersAsync(domain, cancellationToken);

                _logger.LogInformation("Headers check GET response sent: Domain={Domain}, Status={Status}, Score={Score}",
                    result.Domain, result.Status, result.OverallScore);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during headers check GET: {Domain}", domain);
                return StatusCode(500, new { message = "Headers check could not be performed", error = ex.Message });
            }
        }
    }
}
