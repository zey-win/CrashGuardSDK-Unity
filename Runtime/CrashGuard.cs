using System;
using System.Collections.Generic;
using UnityEngine;
using CrashGuard.Internal;

namespace CrashGuard
{
    /// <summary>
    /// CrashGuard - Lightweight crash analytics and stability monitoring SDK
    ///
    /// CrashGuard helps you monitor app stability, detect potential crash conditions,
    /// and analyze runtime environment for crash prevention.
    ///
    /// Usage:
    /// <code>
    /// // Initialize in your startup script
    /// CrashGuard.Start();
    ///
    /// // Check stability status
    /// CrashGuard.AnalyzeStability(result => {
    ///     if (result.requiresAttention) {
    ///         // Environment may cause instability
    ///     }
    /// });
    /// </code>
    /// </summary>
    public static class CrashGuard
    {
        private static bool _isInitialized;
        private static AnalysisResult _lastAnalysis;
        private static string _crashSignature;

        /// <summary>
        /// Whether the SDK is initialized
        /// </summary>
        public static bool IsInitialized => _isInitialized;

        /// <summary>
        /// Last stability analysis result (null if not yet analyzed)
        /// </summary>
        public static AnalysisResult LastAnalysis => _lastAnalysis;

        /// <summary>
        /// Crash signature from last analysis (null if not yet analyzed)
        /// </summary>
        public static string CrashSignature => _crashSignature;

        // ============== Initialization ==============

        /// <summary>
        /// Initialize the CrashGuard SDK
        /// Call this early in your app startup (e.g., in Awake or Start)
        /// </summary>
        public static void Start()
        {
            if (_isInitialized)
            {
                CrashGuardLogger.Warn("Already initialized");
                return;
            }

            CrashGuardBridge.Start();
            _isInitialized = true;

            CrashGuardLogger.Log("Initialized");
        }

        // ============== Stability Analysis ==============

        /// <summary>
        /// Analyzes current environment for potential stability issues
        /// Sends diagnostic data to crash analytics server
        /// </summary>
        /// <param name="onComplete">Callback with analysis result</param>
        public static void AnalyzeStability(Action<AnalysisResult> onComplete)
        {
            EnsureInitialized();

            CrashGuardBridge.AnalyzeStability(json =>
            {
                var result = AnalysisResult.FromJson(json);
                _lastAnalysis = result;
                if (result.IsSuccess)
                {
                    _crashSignature = result.signature;
                }
                onComplete?.Invoke(result);
            });
        }

        /// <summary>
        /// Quick stability check based on cached data or new analysis
        /// </summary>
        /// <param name="onComplete">Callback with result (true if requires attention)</param>
        public static void RequiresAttention(Action<bool> onComplete)
        {
            EnsureInitialized();

            if (_lastAnalysis != null)
            {
                onComplete?.Invoke(_lastAnalysis.requiresAttention);
                return;
            }

            CrashGuardBridge.RequiresAttention(onComplete);
        }

        /// <summary>
        /// Quick cached check for requires attention (synchronous)
        /// Returns false if not yet analyzed
        /// </summary>
        public static bool RequiresAttentionCached()
        {
            return _lastAnalysis?.requiresAttention ?? CrashGuardBridge.RequiresAttentionCached();
        }

        /// <summary>
        /// Verifies crash signature against known problematic signatures
        /// </summary>
        /// <param name="signature">Crash signature to verify</param>
        /// <param name="onComplete">Callback with verification result</param>
        public static void VerifySignature(string signature, Action<SignatureVerification> onComplete)
        {
            EnsureInitialized();

            CrashGuardBridge.VerifySignature(signature, json =>
            {
                var result = SignatureVerification.FromJson(json);
                onComplete?.Invoke(result);
            });
        }

        /// <summary>
        /// Get current runtime environment information
        /// </summary>
        /// <param name="onComplete">Callback with runtime info</param>
        public static void GetRuntimeInfo(Action<RuntimeInfo> onComplete)
        {
            EnsureInitialized();

            CrashGuardBridge.GetRuntimeInfo(json =>
            {
                var result = RuntimeInfo.FromJson(json);
                onComplete?.Invoke(result);
            });
        }

        // ============== Quick Checks (synchronous, no network) ==============

        /// <summary>
        /// Quick check for debug/test environment
        /// </summary>
        public static bool IsDebugEnvironment()
        {
            return CrashGuardBridge.IsDebugEnvironment();
        }

        /// <summary>
        /// Quick check for beta build (TestFlight on iOS)
        /// Always returns false on Android
        /// </summary>
        public static bool IsBetaBuild()
        {
            return CrashGuardBridge.IsBetaBuild();
        }

        /// <summary>
        /// Quick check for runtime modifications (jailbreak/root)
        /// </summary>
        public static bool HasRuntimeModifications()
        {
            return CrashGuardBridge.HasRuntimeModifications();
        }

        /// <summary>
        /// Quick check for emulator/simulator
        /// </summary>
        public static bool IsEmulated()
        {
            return CrashGuardBridge.IsEmulated();
        }

        // ============== Device Identifiers ==============

        /// <summary>
        /// Persistent device identifier (survives reinstalls)
        /// </summary>
        public static string GetDeviceId()
        {
            return CrashGuardBridge.GetPersistentId();
        }

        /// <summary>
        /// Vendor identifier (iOS only, null on Android)
        /// </summary>
        public static string GetVendorId()
        {
            return CrashGuardBridge.GetVendorId();
        }

        /// <summary>
        /// Get crash signature (from last analysis)
        /// Returns null if not yet analyzed
        /// </summary>
        public static string GetCrashSignature()
        {
            return _crashSignature ?? CrashGuardBridge.GetCrashSignature();
        }

        // ============== Event Tracking ==============

        /// <summary>
        /// Records a screen transition for crash context
        /// </summary>
        /// <param name="to">Current screen name</param>
        /// <param name="from">Previous screen name (optional)</param>
        public static void RecordScreenTransition(string to, string from = null)
        {
            EnsureInitialized();
            CrashGuardBridge.RecordScreenTransition(to, from);
        }

        /// <summary>
        /// Records a transaction view event
        /// Call when user views payment/subscription UI
        /// </summary>
        /// <param name="screen">Screen identifier (default: "payment")</param>
        public static void RecordTransactionView(string screen = "payment")
        {
            EnsureInitialized();
            CrashGuardBridge.RecordTransactionView(screen);
        }

        /// <summary>
        /// Records transaction dismissal
        /// Call when user dismisses payment UI without completing
        /// </summary>
        /// <param name="screen">Screen identifier (default: "payment")</param>
        public static void RecordTransactionDismiss(string screen = "payment")
        {
            EnsureInitialized();
            CrashGuardBridge.RecordTransactionDismiss(screen);
        }

        /// <summary>
        /// Records transaction attempt
        /// Call when user initiates a transaction
        /// </summary>
        /// <param name="screen">Screen identifier (default: "payment")</param>
        public static void RecordTransactionAttempt(string screen = "payment")
        {
            EnsureInitialized();
            CrashGuardBridge.RecordTransactionAttempt(screen);
        }

        /// <summary>
        /// Records successful transaction
        /// Call after transaction completes successfully
        /// </summary>
        /// <param name="screen">Screen identifier (default: "payment")</param>
        public static void RecordTransactionSuccess(string screen = "payment")
        {
            EnsureInitialized();
            CrashGuardBridge.RecordTransactionSuccess(screen);
        }

        /// <summary>
        /// Records permission request for crash context
        /// </summary>
        /// <param name="permissionType">Type of permission (e.g., "camera", "location")</param>
        public static void RecordPermissionRequest(string permissionType)
        {
            EnsureInitialized();
            CrashGuardBridge.RecordPermissionRequest(permissionType);
        }

        /// <summary>
        /// Records settings screen access
        /// </summary>
        /// <param name="screen">Screen identifier (default: "settings")</param>
        public static void RecordSettingsAccess(string screen = "settings")
        {
            EnsureInitialized();
            CrashGuardBridge.RecordSettingsAccess(screen);
        }

        /// <summary>
        /// Records a custom diagnostic event
        /// </summary>
        /// <param name="name">Event name</param>
        /// <param name="data">Additional context data (optional)</param>
        public static void RecordEvent(string name, Dictionary<string, string> data = null)
        {
            EnsureInitialized();

            string contextJson = null;
            if (data != null && data.Count > 0)
            {
                contextJson = DictionaryToJson(data);
            }

            CrashGuardBridge.RecordEvent(name, contextJson);
        }

        /// <summary>
        /// Flushes all queued events to server
        /// </summary>
        /// <param name="onComplete">Callback with success status</param>
        public static void FlushEvents(Action<bool> onComplete = null)
        {
            EnsureInitialized();
            CrashGuardBridge.FlushEvents(onComplete ?? (_ => { }));
        }

        // ============== Private Helpers ==============

        private static void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                Start();
            }
        }

        private static string DictionaryToJson(Dictionary<string, string> dict)
        {
            if (dict == null || dict.Count == 0) return "{}";

            var parts = new List<string>();
            foreach (var kvp in dict)
            {
                var key = EscapeJsonString(kvp.Key);
                var value = EscapeJsonString(kvp.Value);
                parts.Add($"\"{key}\":\"{value}\"");
            }
            return "{" + string.Join(",", parts) + "}";
        }

        private static string EscapeJsonString(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            return str
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }
    }
}
