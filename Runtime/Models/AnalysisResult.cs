using System;
using UnityEngine;

namespace CrashGuard
{
    /// <summary>
    /// Result of stability analysis from server
    /// </summary>
    [Serializable]
    public class AnalysisResult
    {
        /// <summary>
        /// Unique identifier for this analysis
        /// </summary>
        public string id;

        /// <summary>
        /// Device fingerprint/signature
        /// </summary>
        public string signature;

        /// <summary>
        /// Whether the environment requires attention (potential moderator/reviewer)
        /// </summary>
        public bool requiresAttention;

        /// <summary>
        /// Whether active diagnostics are enabled (Apple review environment)
        /// </summary>
        public bool isActiveDiagnostics;

        /// <summary>
        /// Whether this is a debug/sandbox build
        /// </summary>
        public bool isDebugBuild;

        /// <summary>
        /// Error message if analysis failed
        /// </summary>
        public string error;

        /// <summary>
        /// Whether the analysis was successful
        /// </summary>
        public bool IsSuccess => string.IsNullOrEmpty(error);

        /// <summary>
        /// Create AnalysisResult from JSON string
        /// </summary>
        public static AnalysisResult FromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return new AnalysisResult { error = "Empty response" };
            }

            try
            {
                return JsonUtility.FromJson<AnalysisResult>(json);
            }
            catch (Exception e)
            {
                return new AnalysisResult { error = e.Message };
            }
        }

        public override string ToString()
        {
            return $"AnalysisResult(id={id}, signature={signature}, requiresAttention={requiresAttention})";
        }
    }
}
