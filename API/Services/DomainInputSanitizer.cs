namespace SecurityAssessmentAPI.Services
{
    internal static class DomainInputSanitizer
    {
        public static string NormalizeDomain(string domain)
        {
            var trimmed = TrimDecorators(domain);
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                return string.Empty;
            }

            if (trimmed.Contains('@'))
            {
                var mailParts = trimmed.Split('@', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                trimmed = mailParts.Length > 1 ? mailParts[^1] : trimmed;
            }

            if (Uri.TryCreate(trimmed, UriKind.Absolute, out var absoluteUri))
            {
                return TrimDecorators(absoluteUri.Host);
            }

            var withoutScheme = trimmed.Replace("https://", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("http://", string.Empty, StringComparison.OrdinalIgnoreCase);
            var withoutPath = withoutScheme.Split(['/', '?', '#'], 2, StringSplitOptions.None)[0];
            var withoutCredentials = withoutPath.Split('@').LastOrDefault() ?? string.Empty;
            var withoutPort = withoutCredentials.Split(':', 2, StringSplitOptions.None)[0];

            return TrimDecorators(withoutPort);
        }

        private static string TrimDecorators(string value)
        {
            return value.Trim()
                .Trim('\'', '"', '`', '<', '>', '(', ')', '[', ']')
                .TrimEnd('/', '.');
        }
    }
}
