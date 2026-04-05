using SecurityAssessmentAPI.DTOs;

namespace SecurityAssessmentAPI.Services
{
    public interface IHeadersCheckingService
    {
        Task<HeadersCheckResult> CheckHeadersAsync(string domain, CancellationToken cancellationToken = default);
    }

    public class HeadersCheckingService : IHeadersCheckingService
    {
        private readonly IMozillaObservatoryClient _mozillaObservatoryClient;
        private readonly IHttpHeadersProbeClient _httpHeadersProbeClient;
        private readonly ILogger<HeadersCheckingService> _logger;

        public HeadersCheckingService(
            IMozillaObservatoryClient mozillaObservatoryClient,
            IHttpHeadersProbeClient httpHeadersProbeClient,
            ILogger<HeadersCheckingService> logger)
        {
            _mozillaObservatoryClient = mozillaObservatoryClient;
            _httpHeadersProbeClient = httpHeadersProbeClient;
            _logger = logger;
        }

        public async Task<HeadersCheckResult> CheckHeadersAsync(string domain, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Headers check started: {Domain}", domain);

            var normalizedDomain = NormalizeDomain(domain);
            // Combine a third-party benchmark with a live probe so missing Observatory data does not block scoring.
            var observatoryTask = _mozillaObservatoryClient.ScanAsync(normalizedDomain, cancellationToken);
            var httpsProbeTask = _httpHeadersProbeClient.ProbeAsync(normalizedDomain, Uri.UriSchemeHttps, cancellationToken);

            await Task.WhenAll(observatoryTask, httpsProbeTask);

            var observatory = await observatoryTask;
            var probe = await httpsProbeTask;

            if (probe == null)
            {
                var httpProbe = await _httpHeadersProbeClient.ProbeAsync(normalizedDomain, Uri.UriSchemeHttp, cancellationToken);
                if (httpProbe != null)
                {
                    if (string.Equals(httpProbe.FinalUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
                    {
                        var redirectedHttpsResult = BuildResult(normalizedDomain, httpProbe, observatory);
                        redirectedHttpsResult.Alerts.Add(new HeadersAlert
                        {
                            Type = "INFO",
                            Message = "The site was reachable via HTTP first and redirected to HTTPS before headers were evaluated."
                        });

                        _logger.LogInformation("Headers check completed after HTTP-to-HTTPS redirect: Domain={Domain}, Score={Score}, Status={Status}",
                            redirectedHttpsResult.Domain, redirectedHttpsResult.OverallScore, redirectedHttpsResult.Status);

                        return redirectedHttpsResult;
                    }

                    return CreateHttpOnlyFailResult(normalizedDomain, httpProbe);
                }

                return CreateErrorResult(normalizedDomain, "The target could not be reached over HTTPS or HTTP, so headers could not be analyzed.");
            }

            if (!string.Equals(probe.FinalUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                return CreateHttpOnlyFailResult(normalizedDomain, probe);
            }

            var result = BuildResult(normalizedDomain, probe, observatory);

            _logger.LogInformation("Headers check completed: Domain={Domain}, Score={Score}, Status={Status}",
                result.Domain, result.OverallScore, result.Status);

            return result;
        }

        private HeadersCheckResult BuildResult(
            string domain,
            HttpHeadersProbeResult probe,
            MozillaObservatoryScanResponse? observatory)
        {
            var result = new HeadersCheckResult
            {
                Domain = domain,
                Observatory = new HeadersObservatorySummary
                {
                    Grade = observatory?.Grade ?? "UNAVAILABLE",
                    Score = observatory?.Score ?? 0,
                    TestsPassed = observatory?.TestsPassed ?? 0,
                    TestsFailed = observatory?.TestsFailed ?? 0,
                    TestsQuantity = observatory?.TestsQuantity ?? 0,
                    DetailsUrl = observatory?.DetailsUrl ?? string.Empty
                }
            };

            var headers = probe.Headers;

            result.Criteria.StrictTransportSecurity = EvaluateStrictTransportSecurity(headers);
            result.Criteria.ContentSecurityPolicy = EvaluateContentSecurityPolicy(headers);
            result.Criteria.ClickjackingProtection = EvaluateClickjackingProtection(headers);
            result.Criteria.MimeSniffingProtection = EvaluateMimeSniffingProtection(headers);
            result.Criteria.ReferrerPolicy = EvaluateReferrerPolicy(headers);

            // Keep the score aligned with the documented model: CSP (4), clickjacking protection (3), and HSTS (3).
            result.OverallScore =
                result.Criteria.StrictTransportSecurity.Score +
                result.Criteria.ContentSecurityPolicy.Score +
                result.Criteria.ClickjackingProtection.Score;

            result.Status = result.OverallScore >= 8 ? "PASS" : result.OverallScore >= 4 ? "WARNING" : "FAIL";

            AddAlerts(result, headers, observatory, probe);

            return result;
        }

        private static HeaderScoreDetail EvaluateStrictTransportSecurity(IReadOnlyDictionary<string, string> headers)
        {
            if (!headers.TryGetValue("Strict-Transport-Security", out var value))
            {
                return new HeaderScoreDetail
                {
                    Score = 0,
                    Details = "Strict-Transport-Security header is missing."
                };
            }

            if (value.Contains("max-age=63072000", StringComparison.OrdinalIgnoreCase) ||
                value.Contains("max-age=31536000", StringComparison.OrdinalIgnoreCase))
            {
                return new HeaderScoreDetail
                {
                    Score = 3,
                    Details = $"Strict-Transport-Security is configured: {value}"
                };
            }

            return new HeaderScoreDetail
            {
                Score = 1,
                Details = $"Strict-Transport-Security is present, but the policy may be weaker than recommended: {value}"
            };
        }

        private static HeaderScoreDetail EvaluateContentSecurityPolicy(IReadOnlyDictionary<string, string> headers)
        {
            if (!headers.TryGetValue("Content-Security-Policy", out var value))
            {
                return new HeaderScoreDetail
                {
                    Score = 0,
                    Details = "Content-Security-Policy header is missing."
                };
            }

            if (!value.Contains("unsafe-inline", StringComparison.OrdinalIgnoreCase) &&
                !value.Contains("unsafe-eval", StringComparison.OrdinalIgnoreCase))
            {
                return new HeaderScoreDetail
                {
                    Score = 4,
                    Details = "Content-Security-Policy is present and does not include unsafe-inline or unsafe-eval."
                };
            }

            var unsafeDirectives = GetUnsafeCspDirectives(value);

            return new HeaderScoreDetail
            {
                Score = 2,
                Details = $"Content-Security-Policy is present, but contains unsafe directives: {string.Join(", ", unsafeDirectives)}."
            };
        }

        private static HeaderScoreDetail EvaluateClickjackingProtection(IReadOnlyDictionary<string, string> headers)
        {
            if (headers.TryGetValue("Content-Security-Policy", out var cspValue) &&
                cspValue.Contains("frame-ancestors", StringComparison.OrdinalIgnoreCase))
            {
                return new HeaderScoreDetail
                {
                    Score = 3,
                    Details = "Clickjacking protection is configured via CSP frame-ancestors."
                };
            }

            if (headers.TryGetValue("X-Frame-Options", out var xfoValue))
            {
                return new HeaderScoreDetail
                {
                    Score = 3,
                    Details = $"X-Frame-Options is present: {xfoValue}"
                };
            }

            return new HeaderScoreDetail
            {
                Score = 0,
                Details = "Neither X-Frame-Options nor CSP frame-ancestors was found."
            };
        }

        private static HeaderScoreDetail EvaluateMimeSniffingProtection(IReadOnlyDictionary<string, string> headers)
        {
            if (headers.TryGetValue("X-Content-Type-Options", out var value) &&
                value.Contains("nosniff", StringComparison.OrdinalIgnoreCase))
            {
                return new HeaderScoreDetail
                {
                    Score = 0,
                    Details = $"X-Content-Type-Options is configured: {value}"
                };
            }

            return new HeaderScoreDetail
            {
                Score = 0,
                Details = "X-Content-Type-Options header with nosniff was not found."
            };
        }

        private static HeaderScoreDetail EvaluateReferrerPolicy(IReadOnlyDictionary<string, string> headers)
        {
            if (!headers.TryGetValue("Referrer-Policy", out var value))
            {
                return new HeaderScoreDetail
                {
                    Score = 0,
                    Details = "Referrer-Policy header is missing."
                };
            }

            if (value.Contains("strict-origin-when-cross-origin", StringComparison.OrdinalIgnoreCase) ||
                value.Contains("no-referrer", StringComparison.OrdinalIgnoreCase) ||
                value.Contains("same-origin", StringComparison.OrdinalIgnoreCase))
            {
                return new HeaderScoreDetail
                {
                    Score = 0,
                    Details = $"Referrer-Policy is configured: {value}"
                };
            }

            return new HeaderScoreDetail
            {
                Score = 0,
                Details = $"Referrer-Policy is present, but may be weaker than recommended: {value}"
            };
        }

        private static void AddAlerts(
            HeadersCheckResult result,
            IReadOnlyDictionary<string, string> headers,
            MozillaObservatoryScanResponse? observatory,
            HttpHeadersProbeResult probe)
        {
            if (!headers.TryGetValue("Strict-Transport-Security", out var hstsValue))
            {
                result.Alerts.Add(new HeadersAlert
                {
                    Type = "CRITICAL_WARNING",
                    Message = "HSTS is missing. Browsers may allow insecure downgrade attempts."
                });
            }
            else if (!hstsValue.Contains("max-age=63072000", StringComparison.OrdinalIgnoreCase) &&
                     !hstsValue.Contains("max-age=31536000", StringComparison.OrdinalIgnoreCase))
            {
                result.Alerts.Add(new HeadersAlert
                {
                    Type = "WARNING",
                    Message = $"HSTS is present, but the policy may be weaker than recommended: {hstsValue}"
                });
            }

            if (!headers.ContainsKey("Content-Security-Policy"))
            {
                result.Alerts.Add(new HeadersAlert
                {
                    Type = "CRITICAL_WARNING",
                    Message = "Content-Security-Policy is missing. Browser-side injection protections are limited."
                });
            }

            if (headers.TryGetValue("Content-Security-Policy", out var cspValue))
            {
                var unsafeDirectives = GetUnsafeCspDirectives(cspValue);
                if (unsafeDirectives.Count > 0)
                {
                result.Alerts.Add(new HeadersAlert
                {
                    Type = "WARNING",
                    Message = $"Content-Security-Policy contains unsafe directives: {string.Join(", ", unsafeDirectives)}."
                });
            }
            }

            if (!headers.TryGetValue("Referrer-Policy", out var referrerPolicyValue))
            {
                result.Alerts.Add(new HeadersAlert
                {
                    Type = "INFO",
                    Message = "Referrer-Policy is missing. Browsers may send more referrer information than necessary."
                });
            }
            else if (!referrerPolicyValue.Contains("strict-origin-when-cross-origin", StringComparison.OrdinalIgnoreCase) &&
                     !referrerPolicyValue.Contains("no-referrer", StringComparison.OrdinalIgnoreCase) &&
                     !referrerPolicyValue.Contains("same-origin", StringComparison.OrdinalIgnoreCase))
            {
                result.Alerts.Add(new HeadersAlert
                {
                    Type = "INFO",
                    Message = $"Referrer-Policy is present, but may be weaker than recommended: {referrerPolicyValue}"
                });
            }

            if (observatory == null)
            {
                result.Alerts.Add(new HeadersAlert
                {
                    Type = "INFO",
                    Message = "Mozilla Observatory data was unavailable, so the result is based on direct header inspection only."
                });
            }
            else if (observatory.TestsFailed > 0)
            {
                result.Alerts.Add(new HeadersAlert
                {
                    Type = "INFO",
                    Message = $"Mozilla Observatory reports {observatory.TestsFailed} failed test(s) out of {observatory.TestsQuantity}."
                });
            }

            if (!string.Equals(probe.FinalUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                result.Alerts.Add(new HeadersAlert
                {
                    Type = "CRITICAL_WARNING",
                    Message = $"The final response was not served over HTTPS: {probe.FinalUri}"
                });
            }

            if (!string.Equals(probe.FinalUri.Host, result.Domain, StringComparison.OrdinalIgnoreCase))
            {
                result.Alerts.Add(new HeadersAlert
                {
                    Type = "INFO",
                    Message = $"The request was redirected from {result.Domain} to {probe.FinalUri.Host}."
                });
            }
        }

        private static string NormalizeDomain(string domain)
        {
            var trimmed = domain.Trim();

            if (Uri.TryCreate(trimmed, UriKind.Absolute, out var absoluteUri))
            {
                return absoluteUri.Host;
            }

            return trimmed.Replace("https://", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("http://", string.Empty, StringComparison.OrdinalIgnoreCase)
                .TrimEnd('/');
        }

        private static List<string> GetUnsafeCspDirectives(string cspValue)
        {
            var unsafeDirectives = new List<string>();

            if (cspValue.Contains("unsafe-inline", StringComparison.OrdinalIgnoreCase))
            {
                unsafeDirectives.Add("unsafe-inline");
            }

            if (cspValue.Contains("unsafe-eval", StringComparison.OrdinalIgnoreCase))
            {
                unsafeDirectives.Add("unsafe-eval");
            }

            return unsafeDirectives;
        }

        private static HeadersCheckResult CreateErrorResult(string domain, string message)
        {
            return new HeadersCheckResult
            {
                Domain = domain,
                Status = "ERROR",
                Alerts = new List<HeadersAlert>
                {
                    new HeadersAlert
                    {
                        Type = "CRITICAL_ALARM",
                        Message = message
                    }
                }
            };
        }

        private static HeadersCheckResult CreateHttpOnlyFailResult(string domain, HttpHeadersProbeResult probe)
        {
            return new HeadersCheckResult
            {
                Domain = domain,
                OverallScore = 0,
                Status = "FAIL",
                Alerts = new List<HeadersAlert>
                {
                    new HeadersAlert
                    {
                        Type = "CRITICAL_ALARM",
                        Message = $"The target is not served over HTTPS. Final response: {probe.FinalUri}"
                    }
                }
            };
        }
    }
}
