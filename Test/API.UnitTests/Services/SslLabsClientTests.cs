using System.Net;
using API.UnitTests.TestSupport;
using Microsoft.Extensions.Logging.Abstractions;
using SecurityAssessmentAPI.Services;
using Xunit;

namespace API.UnitTests.Services;

public sealed class SslLabsClientTests
{
    [Fact]
    public async Task AnalyzeAsync_WhenResponseIsValid_ParsesEndpointsCertificatesAndSuites()
    {
        var handler = new StubHttpMessageHandler((request, _) =>
            Task.FromResult(HttpResponseFactory.Json(HttpStatusCode.OK, """
            {
              "status": "READY",
              "host": "example.com",
              "certs": [
                {
                  "notBefore": 1700000000000,
                  "notAfter": 1800000000000,
                  "issuerSubject": "CN=Issuer",
                  "subject": "CN=example.com",
                  "sha256Hash": "abc123",
                  "sigAlg": "sha256WithRSAEncryption",
                  "keyAlg": "RSA",
                  "keySize": 2048,
                  "commonNames": ["example.com"],
                  "altNames": ["www.example.com"]
                }
              ],
              "endpoints": [
                {
                  "ipAddress": "203.0.113.10",
                  "serverName": "example.com",
                  "grade": "A",
                  "details": {
                    "protocols": [
                      { "name": "TLS", "version": "1.3" }
                    ],
                    "namedGroups": [
                      { "name": "X25519", "bits": 253 }
                    ],
                    "suites": [
                      {
                        "protocol": "TLS 1.3",
                        "list": [
                          {
                            "name": "TLS_AES_256_GCM_SHA384",
                            "cipherStrength": 256,
                            "namedGroupName": "X25519"
                          }
                        ]
                      }
                    ]
                  }
                }
              ]
            }
            """, request.RequestUri)));

        var client = new SslLabsClient(new HttpClient(handler), NullLogger<SslLabsClient>.Instance);

        var result = await client.AnalyzeAsync("example.com");

        Assert.Equal("READY", result.Status);
        Assert.Single(result.Certs);
        Assert.Single(result.Endpoints);
        Assert.Single(result.Endpoints[0].Details.Protocols);
        Assert.Single(result.Endpoints[0].Details.NamedGroups);
        Assert.Single(result.Endpoints[0].Details.Suites);
    }

    [Fact]
    public async Task AnalyzeAsync_WhenResponseIsEmpty_ThrowsInvalidOperationException()
    {
        var handler = new StubHttpMessageHandler((request, _) =>
            Task.FromResult(HttpResponseFactory.Empty(HttpStatusCode.OK, request.RequestUri)));

        var client = new SslLabsClient(new HttpClient(handler), NullLogger<SslLabsClient>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() => client.AnalyzeAsync("example.com"));
    }
}
