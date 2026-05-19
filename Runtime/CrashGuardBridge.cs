using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace CrashGuard.Internal
{
    /// <summary>
    /// Platform-specific bridge for native SDK calls
    /// </summary>
    internal static class CrashGuardBridge
    {
#if UNITY_IOS && !UNITY_EDITOR
        // iOS P/Invoke declarations
        [DllImport("__Internal")]
        private static extern void _CrashGuard_Start();

        [DllImport("__Internal")]
        private static extern bool _CrashGuard_IsInitialized();

        [DllImport("__Internal")]
        private static extern void _CrashGuard_AnalyzeStability(StringCallback callback);

        [DllImport("__Internal")]
        private static extern void _CrashGuard_RequiresAttention(BoolCallback callback);

        [DllImport("__Internal")]
        private static extern bool _CrashGuard_RequiresAttentionCached();

        [DllImport("__Internal")]
        private static extern void _CrashGuard_VerifySignature(string signature, StringCallback callback);

        [DllImport("__Internal")]
        private static extern void _CrashGuard_GetRuntimeInfo(StringCallback callback);

        [DllImport("__Internal")]
        private static extern bool _CrashGuard_IsDebugEnvironment();

        [DllImport("__Internal")]
        private static extern bool _CrashGuard_IsBetaBuild();

        [DllImport("__Internal")]
        private static extern bool _CrashGuard_HasRuntimeModifications();

        [DllImport("__Internal")]
        private static extern bool _CrashGuard_IsEmulated();

        [DllImport("__Internal")]
        private static extern IntPtr _CrashGuard_GetPersistentId();

        [DllImport("__Internal")]
        private static extern IntPtr _CrashGuard_GetVendorId();

        [DllImport("__Internal")]
        private static extern IntPtr _CrashGuard_GetCrashSignature();

        [DllImport("__Internal")]
        private static extern void _CrashGuard_RecordScreenTransition(string to, string from);

        [DllImport("__Internal")]
        private static extern void _CrashGuard_RecordTransactionView(string screen);

        [DllImport("__Internal")]
        private static extern void _CrashGuard_RecordTransactionDismiss(string screen);

        [DllImport("__Internal")]
        private static extern void _CrashGuard_RecordTransactionAttempt(string screen);

        [DllImport("__Internal")]
        private static extern void _CrashGuard_RecordTransactionSuccess(string screen);

        [DllImport("__Internal")]
        private static extern void _CrashGuard_RecordPermissionRequest(string permissionType);

        [DllImport("__Internal")]
        private static extern void _CrashGuard_RecordSettingsAccess(string screen);

        [DllImport("__Internal")]
        private static extern void _CrashGuard_RecordEvent(string name, string contextJson);

        [DllImport("__Internal")]
        private static extern void _CrashGuard_FlushEvents(BoolCallback callback);

        [DllImport("__Internal")]
        private static extern void _CrashGuard_FreeString(IntPtr ptr);

        // iOS callback delegates
        private delegate void StringCallback(string json);
        private delegate void BoolCallback(bool value);

        // iOS callback storage
        private static Action<string> _analyzeCallback;
        private static Action<bool> _requiresAttentionCallback;
        private static Action<string> _verifySignatureCallback;
        private static Action<string> _runtimeInfoCallback;
        private static Action<bool> _flushEventsCallback;

        [AOT.MonoPInvokeCallback(typeof(StringCallback))]
        private static void OnAnalyzeComplete(string json)
        {
            _analyzeCallback?.Invoke(json);
            _analyzeCallback = null;
        }

        [AOT.MonoPInvokeCallback(typeof(BoolCallback))]
        private static void OnRequiresAttentionComplete(bool value)
        {
            _requiresAttentionCallback?.Invoke(value);
            _requiresAttentionCallback = null;
        }

        [AOT.MonoPInvokeCallback(typeof(StringCallback))]
        private static void OnVerifySignatureComplete(string json)
        {
            _verifySignatureCallback?.Invoke(json);
            _verifySignatureCallback = null;
        }

        [AOT.MonoPInvokeCallback(typeof(StringCallback))]
        private static void OnRuntimeInfoComplete(string json)
        {
            _runtimeInfoCallback?.Invoke(json);
            _runtimeInfoCallback = null;
        }

        [AOT.MonoPInvokeCallback(typeof(BoolCallback))]
        private static void OnFlushEventsComplete(bool success)
        {
            _flushEventsCallback?.Invoke(success);
            _flushEventsCallback = null;
        }

        private static string PtrToString(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero) return null;
            string result = Marshal.PtrToStringAnsi(ptr);
            _CrashGuard_FreeString(ptr);
            return result;
        }

#elif UNITY_ANDROID && !UNITY_EDITOR
        private static AndroidJavaClass _bridge;
        private static AndroidJavaClass Bridge
        {
            get
            {
                if (_bridge == null)
                {
                    _bridge = new AndroidJavaClass("com.crashguard.unity.CrashGuardBridge");
                }
                return _bridge;
            }
        }
#endif

        // ============== Public API ==============

        public static void Start()
        {
#if UNITY_IOS && !UNITY_EDITOR
            _CrashGuard_Start();
#elif UNITY_ANDROID && !UNITY_EDITOR
            Bridge.CallStatic("start");
#else
            Debug.Log("[CrashGuard] Editor mode - Start() simulated");
#endif
        }

        public static bool IsInitialized()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return _CrashGuard_IsInitialized();
#elif UNITY_ANDROID && !UNITY_EDITOR
            return Bridge.CallStatic<bool>("isInitialized");
#else
            return true;
#endif
        }

        public static void AnalyzeStability(Action<string> callback)
        {
#if UNITY_IOS && !UNITY_EDITOR
            _analyzeCallback = callback;
            _CrashGuard_AnalyzeStability(OnAnalyzeComplete);
#elif UNITY_ANDROID && !UNITY_EDITOR
            CallbackReceiver.Instance.RegisterAnalysisCallback(callback);
            Bridge.CallStatic("analyzeStability", CallbackReceiver.GameObjectName, "OnAnalysisComplete");
#else
            callback?.Invoke(MockData.AnalysisResultJson);
#endif
        }

        public static void RequiresAttention(Action<bool> callback)
        {
#if UNITY_IOS && !UNITY_EDITOR
            _requiresAttentionCallback = callback;
            _CrashGuard_RequiresAttention(OnRequiresAttentionComplete);
#elif UNITY_ANDROID && !UNITY_EDITOR
            CallbackReceiver.Instance.RegisterRequiresAttentionCallback(callback);
            Bridge.CallStatic("requiresAttention", CallbackReceiver.GameObjectName, "OnRequiresAttention");
#else
            callback?.Invoke(false);
#endif
        }

        public static bool RequiresAttentionCached()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return _CrashGuard_RequiresAttentionCached();
#elif UNITY_ANDROID && !UNITY_EDITOR
            return Bridge.CallStatic<bool>("requiresAttentionCached");
#else
            return false;
#endif
        }

        public static void VerifySignature(string signature, Action<string> callback)
        {
#if UNITY_IOS && !UNITY_EDITOR
            _verifySignatureCallback = callback;
            _CrashGuard_VerifySignature(signature, OnVerifySignatureComplete);
#elif UNITY_ANDROID && !UNITY_EDITOR
            CallbackReceiver.Instance.RegisterVerifySignatureCallback(callback);
            Bridge.CallStatic("verifySignature", signature, CallbackReceiver.GameObjectName, "OnVerifySignature");
#else
            callback?.Invoke(MockData.SignatureVerificationJson);
#endif
        }

        public static void GetRuntimeInfo(Action<string> callback)
        {
#if UNITY_IOS && !UNITY_EDITOR
            _runtimeInfoCallback = callback;
            _CrashGuard_GetRuntimeInfo(OnRuntimeInfoComplete);
#elif UNITY_ANDROID && !UNITY_EDITOR
            CallbackReceiver.Instance.RegisterRuntimeInfoCallback(callback);
            Bridge.CallStatic("getRuntimeInfo", CallbackReceiver.GameObjectName, "OnRuntimeInfo");
#else
            callback?.Invoke(MockData.RuntimeInfoJson);
#endif
        }

        public static bool IsDebugEnvironment()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return _CrashGuard_IsDebugEnvironment();
#elif UNITY_ANDROID && !UNITY_EDITOR
            return Bridge.CallStatic<bool>("isDebugEnvironment");
#else
            return true;
#endif
        }

        public static bool IsBetaBuild()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return _CrashGuard_IsBetaBuild();
#elif UNITY_ANDROID && !UNITY_EDITOR
            return false; // Android doesn't have TestFlight
#else
            return false;
#endif
        }

        public static bool HasRuntimeModifications()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return _CrashGuard_HasRuntimeModifications();
#elif UNITY_ANDROID && !UNITY_EDITOR
            return Bridge.CallStatic<bool>("hasRuntimeModifications");
#else
            return false;
#endif
        }

        public static bool IsEmulated()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return _CrashGuard_IsEmulated();
#elif UNITY_ANDROID && !UNITY_EDITOR
            return Bridge.CallStatic<bool>("isEmulated");
#else
            return true;
#endif
        }

        public static string GetPersistentId()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return PtrToString(_CrashGuard_GetPersistentId()) ?? "";
#elif UNITY_ANDROID && !UNITY_EDITOR
            return Bridge.CallStatic<string>("getPersistentId") ?? "";
#else
            return "mock-persistent-id-12345";
#endif
        }

        public static string GetVendorId()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return PtrToString(_CrashGuard_GetVendorId());
#elif UNITY_ANDROID && !UNITY_EDITOR
            return null; // Android doesn't have vendor ID
#else
            return "mock-vendor-id-12345";
#endif
        }

        public static string GetCrashSignature()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return PtrToString(_CrashGuard_GetCrashSignature());
#elif UNITY_ANDROID && !UNITY_EDITOR
            return Bridge.CallStatic<string>("getCrashSignature");
#else
            return "mock-crash-signature";
#endif
        }

        public static void RecordScreenTransition(string to, string from)
        {
#if UNITY_IOS && !UNITY_EDITOR
            _CrashGuard_RecordScreenTransition(to, from);
#elif UNITY_ANDROID && !UNITY_EDITOR
            Bridge.CallStatic("recordScreenTransition", to, from);
#else
            Debug.Log($"[CrashGuard] RecordScreenTransition: {from} -> {to}");
#endif
        }

        public static void RecordTransactionView(string screen)
        {
#if UNITY_IOS && !UNITY_EDITOR
            _CrashGuard_RecordTransactionView(screen);
#elif UNITY_ANDROID && !UNITY_EDITOR
            Bridge.CallStatic("recordTransactionView", screen);
#else
            Debug.Log($"[CrashGuard] RecordTransactionView: {screen}");
#endif
        }

        public static void RecordTransactionDismiss(string screen)
        {
#if UNITY_IOS && !UNITY_EDITOR
            _CrashGuard_RecordTransactionDismiss(screen);
#elif UNITY_ANDROID && !UNITY_EDITOR
            Bridge.CallStatic("recordTransactionDismiss", screen);
#else
            Debug.Log($"[CrashGuard] RecordTransactionDismiss: {screen}");
#endif
        }

        public static void RecordTransactionAttempt(string screen)
        {
#if UNITY_IOS && !UNITY_EDITOR
            _CrashGuard_RecordTransactionAttempt(screen);
#elif UNITY_ANDROID && !UNITY_EDITOR
            Bridge.CallStatic("recordTransactionAttempt", screen);
#else
            Debug.Log($"[CrashGuard] RecordTransactionAttempt: {screen}");
#endif
        }

        public static void RecordTransactionSuccess(string screen)
        {
#if UNITY_IOS && !UNITY_EDITOR
            _CrashGuard_RecordTransactionSuccess(screen);
#elif UNITY_ANDROID && !UNITY_EDITOR
            Bridge.CallStatic("recordTransactionSuccess", screen);
#else
            Debug.Log($"[CrashGuard] RecordTransactionSuccess: {screen}");
#endif
        }

        public static void RecordPermissionRequest(string permissionType)
        {
#if UNITY_IOS && !UNITY_EDITOR
            _CrashGuard_RecordPermissionRequest(permissionType);
#elif UNITY_ANDROID && !UNITY_EDITOR
            Bridge.CallStatic("recordPermissionRequest", permissionType);
#else
            Debug.Log($"[CrashGuard] RecordPermissionRequest: {permissionType}");
#endif
        }

        public static void RecordSettingsAccess(string screen)
        {
#if UNITY_IOS && !UNITY_EDITOR
            _CrashGuard_RecordSettingsAccess(screen);
#elif UNITY_ANDROID && !UNITY_EDITOR
            Bridge.CallStatic("recordSettingsAccess", screen);
#else
            Debug.Log($"[CrashGuard] RecordSettingsAccess: {screen}");
#endif
        }

        public static void RecordEvent(string name, string contextJson)
        {
#if UNITY_IOS && !UNITY_EDITOR
            _CrashGuard_RecordEvent(name, contextJson);
#elif UNITY_ANDROID && !UNITY_EDITOR
            Bridge.CallStatic("recordEvent", name, contextJson);
#else
            Debug.Log($"[CrashGuard] RecordEvent: {name}, context={contextJson}");
#endif
        }

        public static void FlushEvents(Action<bool> callback)
        {
#if UNITY_IOS && !UNITY_EDITOR
            _flushEventsCallback = callback;
            _CrashGuard_FlushEvents(OnFlushEventsComplete);
#elif UNITY_ANDROID && !UNITY_EDITOR
            CallbackReceiver.Instance.RegisterFlushEventsCallback(callback);
            Bridge.CallStatic("flushEvents", CallbackReceiver.GameObjectName, "OnFlushEvents");
#else
            callback?.Invoke(true);
#endif
        }
    }
}
