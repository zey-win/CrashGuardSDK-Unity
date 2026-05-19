using System;
using System.Collections.Generic;
using UnityEngine;

namespace CrashGuard.Internal
{
    /// <summary>
    /// Hidden GameObject that receives callbacks from Android native code via UnitySendMessage
    /// </summary>
    internal class CallbackReceiver : MonoBehaviour
    {
        public const string GameObjectName = "__CrashGuardCallbackReceiver__";

        private static CallbackReceiver _instance;
        public static CallbackReceiver Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject(GameObjectName);
                    DontDestroyOnLoad(go);
                    go.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
                    _instance = go.AddComponent<CallbackReceiver>();
                }
                return _instance;
            }
        }

        // Callback queues
        private readonly Queue<Action<string>> _analysisCallbacks = new Queue<Action<string>>();
        private readonly Queue<Action<bool>> _requiresAttentionCallbacks = new Queue<Action<bool>>();
        private readonly Queue<Action<string>> _verifySignatureCallbacks = new Queue<Action<string>>();
        private readonly Queue<Action<string>> _runtimeInfoCallbacks = new Queue<Action<string>>();
        private readonly Queue<Action<bool>> _flushEventsCallbacks = new Queue<Action<bool>>();

        // Registration methods
        public void RegisterAnalysisCallback(Action<string> callback)
        {
            _analysisCallbacks.Enqueue(callback);
        }

        public void RegisterRequiresAttentionCallback(Action<bool> callback)
        {
            _requiresAttentionCallbacks.Enqueue(callback);
        }

        public void RegisterVerifySignatureCallback(Action<string> callback)
        {
            _verifySignatureCallbacks.Enqueue(callback);
        }

        public void RegisterRuntimeInfoCallback(Action<string> callback)
        {
            _runtimeInfoCallbacks.Enqueue(callback);
        }

        public void RegisterFlushEventsCallback(Action<bool> callback)
        {
            _flushEventsCallbacks.Enqueue(callback);
        }

        // Called from Android via UnitySendMessage
        public void OnAnalysisComplete(string json)
        {
            if (_analysisCallbacks.Count > 0)
            {
                var callback = _analysisCallbacks.Dequeue();
                callback?.Invoke(json);
            }
        }

        // Called from Android via UnitySendMessage
        public void OnRequiresAttention(string value)
        {
            if (_requiresAttentionCallbacks.Count > 0)
            {
                var callback = _requiresAttentionCallbacks.Dequeue();
                callback?.Invoke(value == "true");
            }
        }

        // Called from Android via UnitySendMessage
        public void OnVerifySignature(string json)
        {
            if (_verifySignatureCallbacks.Count > 0)
            {
                var callback = _verifySignatureCallbacks.Dequeue();
                callback?.Invoke(json);
            }
        }

        // Called from Android via UnitySendMessage
        public void OnRuntimeInfo(string json)
        {
            if (_runtimeInfoCallbacks.Count > 0)
            {
                var callback = _runtimeInfoCallbacks.Dequeue();
                callback?.Invoke(json);
            }
        }

        // Called from Android via UnitySendMessage
        public void OnFlushEvents(string value)
        {
            if (_flushEventsCallbacks.Count > 0)
            {
                var callback = _flushEventsCallbacks.Dequeue();
                callback?.Invoke(value == "true");
            }
        }

        private void OnDestroy()
        {
            _instance = null;
        }
    }
}
