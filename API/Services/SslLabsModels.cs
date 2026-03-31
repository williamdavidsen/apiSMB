using System.Text.Json.Serialization;

namespace SecurityAssessmentAPI.Services
{
    public class SslLabsResponse
    {
        public string Status { get; set; } = "IN_PROGRESS";
        public string Host { get; set; } = string.Empty;
        public List<SslLabsEndpoint> Endpoints { get; set; } = new();
        public List<SslLabsCert> Certs { get; set; } = new();
    }

    public class SslLabsEndpoint
    {
        public string IpAddress { get; set; } = string.Empty;
        public string ServerName { get; set; } = string.Empty;
        public string Grade { get; set; } = "F";
        public SslLabsEndpointDetails Details { get; set; } = new();
    }

    public class SslLabsEndpointDetails
    {
        public List<SslLabsProtocol> Protocols { get; set; } = new();
        public List<SslLabsProtocolSuiteGroup> Suites { get; set; } = new();
        public List<SslLabsNamedGroup> NamedGroups { get; set; } = new();
    }

    public class SslLabsProtocolSuiteGroup
    {
        public string Protocol { get; set; } = string.Empty;
        public List<SslLabsSuite> List { get; set; } = new();
    }

    public class SslLabsCert
    {
        public long NotBefore { get; set; }
        public long NotAfter { get; set; }
        public string IssuerSubject { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public List<string> CommonNames { get; set; } = new();
        public List<string> AltNames { get; set; } = new();
    }

    public class SslLabsProtocol
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;

        [JsonPropertyName("v2SuitesDisabled")]
        public bool? V2SuitesDisabled { get; set; }
    }

    public class SslLabsSuite
    {
        public string Name { get; set; } = string.Empty;
        public int CipherStrength { get; set; }
        public int? Q { get; set; }
        public string NamedGroupName { get; set; } = string.Empty;
        public int? NamedGroupBits { get; set; }
    }

    public class SslLabsNamedGroup
    {
        public string Name { get; set; } = string.Empty;
        public int? Bits { get; set; }
    }
}
