namespace CrashGuard.Internal
{
    /// <summary>
    /// Mock data for Editor testing
    /// </summary>
    internal static class MockData
    {
        public const string AnalysisResultJson = @"{
            ""id"": ""mock-analysis-id"",
            ""signature"": ""mock-signature-12345"",
            ""requiresAttention"": false,
            ""isActiveDiagnostics"": false,
            ""isDebugBuild"": true
        }";

        public const string SignatureVerificationJson = @"{
            ""signature"": ""mock-signature-12345"",
            ""isFlagged"": false
        }";

        public const string RuntimeInfoJson = @"{
            ""isDebugEnvironment"": true,
            ""isBetaBuild"": false,
            ""isEmulated"": true,
            ""hasRuntimeModifications"": false,
            ""hasDiagnosticsEnabled"": false,
            ""isDeveloperMode"": false,
            ""isAdbEnabled"": false
        }";
    }
}
