using System;
using UnityEngine;

namespace CrashGuard
{
    /// <summary>
    /// Runtime environment information
    /// </summary>
    [Serializable]
    public class RuntimeInfo
    {
        /// <summary>
        /// True if running in debug/sandbox mode
        /// </summary>
        public bool isDebugEnvironment;

        /// <summary>
        /// True if beta/TestFlight build (iOS only)
        /// </summary>
        public bool isBetaBuild;

        /// <summary>
        /// True if running in emulator/simulator
        /// </summary>
        public bool isEmulated;

        /// <summary>
        /// True if device has root/jailbreak modifications
        /// </summary>
        public bool hasRuntimeModifications;

        /// <summary>
        /// True if developer options are enabled (Android) or diagnostics flags present (iOS)
        /// </summary>
        public bool hasDiagnosticsEnabled;

        /// <summary>
        /// True if developer mode is enabled (Android only)
        /// </summary>
        public bool isDeveloperMode;

        /// <summary>
        /// True if ADB is enabled (Android only)
        /// </summary>
        public bool isAdbEnabled;

        /// <summary>
        /// Create RuntimeInfo from JSON string
        /// </summary>
        public static RuntimeInfo FromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return new RuntimeInfo();
            }

            try
            {
                return JsonUtility.FromJson<RuntimeInfo>(json);
            }
            catch
            {
                return new RuntimeInfo();
            }
        }

        public override string ToString()
        {
            return $"RuntimeInfo(debug={isDebugEnvironment}, beta={isBetaBuild}, emulated={isEmulated}, rooted={hasRuntimeModifications})";
        }
    }
}
