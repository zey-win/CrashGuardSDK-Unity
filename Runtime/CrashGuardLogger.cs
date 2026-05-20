using System;
using UnityEngine;

namespace CrashGuard.Internal
{
    internal enum CrashGuardLogLevel
    {
        None = 0,
        Error = 1,
        Warning = 2,
        Info = 3,
        Debug = 4
    }

    internal static class CrashGuardLogger
    {
        private const string Tag = "[CrashGuard]";
        private static CrashGuardLogLevel _currentLogLevel = CrashGuardLogLevel.Info;

        public static CrashGuardLogLevel CurrentLogLevel
        {
            get => _currentLogLevel;
            set => _currentLogLevel = value;
        }

        public static void Log(string message)
        {
            if (_currentLogLevel >= CrashGuardLogLevel.Info)
                Debug.Log($"{Tag} {message}");
        }

        public static void Log(string format, params object[] args)
        {
            if (_currentLogLevel >= CrashGuardLogLevel.Info)
                Debug.Log($"{Tag} {string.Format(format, args)}");
        }

        public static void Warn(string message)
        {
            if (_currentLogLevel >= CrashGuardLogLevel.Warning)
                Debug.LogWarning($"{Tag} {message}");
        }

        public static void Warn(string format, params object[] args)
        {
            if (_currentLogLevel >= CrashGuardLogLevel.Warning)
                Debug.LogWarning($"{Tag} {string.Format(format, args)}");
        }

        public static void Error(string message)
        {
            if (_currentLogLevel >= CrashGuardLogLevel.Error)
                Debug.LogError($"{Tag} {message}");
        }

        public static void Error(string format, params object[] args)
        {
            if (_currentLogLevel >= CrashGuardLogLevel.Error)
                Debug.LogError($"{Tag} {string.Format(format, args)}");
        }

#if UNITY_EDITOR || CRASHGUARD_DEBUG
        public static void DebugLog(string message)
        {
            if (_currentLogLevel >= CrashGuardLogLevel.Debug)
                Debug.Log($"{Tag} {message}");
        }

        public static void DebugLog(string format, params object[] args)
        {
            if (_currentLogLevel >= CrashGuardLogLevel.Debug)
                Debug.Log($"{Tag} {string.Format(format, args)}");
        }
#else
        public static void DebugLog(string message) { }
        public static void DebugLog(string format, params object[] args) { }
#endif
    }
}
