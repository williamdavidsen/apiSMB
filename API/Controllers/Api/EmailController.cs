using Microsoft.AspNetCore.Mvc;
using SecurityAssessmentAPI.DTOs;
using SecurityAssessmentAPI.Services;

namespace SecurityAssessmentAPI.Controllers.Api
{
    [ApiController]
    [Route("api/email")]
    public class EmailController : ControllerBase
    {
        private readonly IEmailCheckingService _emailCheckingService;
        private readonly ILogger<EmailController> _logger;

        public EmailController(IEmailCheckingService emailCheckingService, ILogger<EmailController> logger)
        {
            _emailCheckingService = emailCheckingService;
            _logger = logger;
        }

        [HttpPost("check")]
        public async Task<IActionResult> CheckEmail([FromBody] EmailCheckRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid email check request: {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))));
                return BadRequest(ModelState);
            }

            try
            {
                _logger.LogInformation("Email check request received: {Domain}", request.Domain);

                var result = await _emailCheckingService.CheckEmailAsync(request.Domain, cancellationToken);

                _logger.LogInformation("Email check response sent: Domain={Domain}, Status={Status}, Score={Score}",
                    result.Domain, result.Status, result.OverallScore);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during email check: {Domain}", request.Domain);
                return StatusCode(500, new { message = "Email security check could not be performed", error = ex.Message });
            }
        }

        [HttpGet("check/{domain}")]
        public async Task<IActionResult> GetEmailCheck(string domain, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                _logger.LogWarning("Invalid domain parameter: {Domain}", domain);
                return BadRequest("Domain is required");
            }

            try
            {
                _logger.LogInformation("Email check GET request received: {Domain}", domain);

                var result = await _emailCheckingService.CheckEmailAsync(domain, cancellationToken);

                _logger.LogInformation("Email check GET response sent: Domain={Domain}, Status={Status}, Score={Score}",
                    result.Domain, result.Status, result.OverallScore);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during email check GET: {Domain}", domain);
                return StatusCode(500, new { message = "Email security check could not be performed", error = ex.Message });
            }
        }
    }
}
