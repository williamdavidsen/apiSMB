namespace SecurityAssessmentAPI.DTOs
{
    public class PqcCheckResult
    {
        public string Domain { get; set; } = string.Empty;
        public bool PqcDetected { get; set; }
        public string Status { get; set; } = "UNKNOWN";
        public string Mode { get; set; } = "Unclear";
        public string ReadinessLevel { get; set; } = "Unknown / not verifiable";
        public string AlgorithmFamily { get; set; } = "Unknown";
        public bool HandshakeSupported { get; set; }
        public string Confidence { get; set; } = "LOW";
        public string Notes { get; set; } = string.Empty;
        public List<string> Evidence { get; set; } = new();
    }
}
