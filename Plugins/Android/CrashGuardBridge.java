package com.crashguard.unity;

import android.app.Activity;
import android.util.Log;

import com.crashguard.sdk.CrashGuard;
import com.crashguard.sdk.RuntimeInfo;
import com.crashguard.sdk.network.AnalysisResult;
import com.crashguard.sdk.network.SignatureVerification;
import com.unity3d.player.UnityPlayer;

import org.json.JSONObject;

import java.util.HashMap;
import java.util.Iterator;
import java.util.Map;

import kotlin.Unit;
import kotlin.Result;

/**
 * Bridge between Unity C# and CrashGuard Android SDK
 */
public class CrashGuardBridge {
    private static final String TAG = "CrashGuardBridge";

    private static Activity getActivity() {
        return UnityPlayer.currentActivity;
    }

    // ============== Initialization ==============

    public static void start() {
        Activity activity = getActivity();
        if (activity != null) {
            CrashGuard.start(activity);
        }
    }

    public static boolean isInitialized() {
        return CrashGuard.isInitialized();
    }

    // ============== Stability Analysis ==============

    public static void analyzeStability(final String gameObject, final String methodName) {
        try {
            CrashGuard.Companion.getShared().analyzeStability(result -> {
                try {
                    if (result.isSuccess()) {
                        AnalysisResult analysis = result.getOrNull();
                        if (analysis != null) {
                            JSONObject json = new JSONObject();
                            json.put("id", analysis.getId());
                            json.put("signature", analysis.getSignature());
                            json.put("requiresAttention", analysis.getRequiresAttention());
                            json.put("isActiveDiagnostics", analysis.isActiveDiagnostics());
                            json.put("isDebugBuild", analysis.isDebugBuild());
                            UnityPlayer.UnitySendMessage(gameObject, methodName, json.toString());
                        }
                    } else {
                        JSONObject json = new JSONObject();
                        Throwable error = result.exceptionOrNull();
                        json.put("error", error != null ? error.getMessage() : "Unknown error");
                        UnityPlayer.UnitySendMessage(gameObject, methodName, json.toString());
                    }
                } catch (Exception e) {
                    Log.e(TAG, "Error in analyzeStability callback", e);
                }
                return Unit.INSTANCE;
            });
        } catch (Exception e) {
            Log.e(TAG, "Error calling analyzeStability", e);
        }
    }

    public static void requiresAttention(final String gameObject, final String methodName) {
        try {
            CrashGuard.Companion.getShared().requiresAttention(result -> {
                try {
                    Boolean requires = result.isSuccess() ? result.getOrNull() : false;
                    UnityPlayer.UnitySendMessage(gameObject, methodName, requires != null && requires ? "true" : "false");
                } catch (Exception e) {
                    Log.e(TAG, "Error in requiresAttention callback", e);
                }
                return Unit.INSTANCE;
            });
        } catch (Exception e) {
            Log.e(TAG, "Error calling requiresAttention", e);
        }
    }

    public static boolean requiresAttentionCached() {
        try {
            AnalysisResult lastAnalysis = CrashGuard.Companion.getShared().getLastAnalysis();
            return lastAnalysis != null && lastAnalysis.getRequiresAttention();
        } catch (Exception e) {
            return false;
        }
    }

    public static void verifySignature(String signature, final String gameObject, final String methodName) {
        // Note: verifySignature is a suspend function in Kotlin
        // We need to handle this appropriately - for now, send error
        try {
            JSONObject json = new JSONObject();
            json.put("error", "verifySignature not yet implemented for Android Unity bridge");
            UnityPlayer.UnitySendMessage(gameObject, methodName, json.toString());
        } catch (Exception e) {
            Log.e(TAG, "Error in verifySignature", e);
        }
    }

    // ============== Runtime Info ==============

    public static void getRuntimeInfo(final String gameObject, final String methodName) {
        try {
            RuntimeInfo info = CrashGuard.Companion.getShared().getRuntimeInfo();
            JSONObject json = new JSONObject();
            json.put("isDebugEnvironment", info.isDebugEnvironment());
            json.put("isEmulated", info.isEmulated());
            json.put("hasRuntimeModifications", info.getHasRuntimeModifications());
            json.put("isDeveloperMode", info.isDeveloperMode());
            json.put("isAdbEnabled", info.isAdbEnabled());
            json.put("isBetaBuild", false); // Android doesn't have TestFlight equivalent
            json.put("hasDiagnosticsEnabled", info.isDeveloperMode());
            UnityPlayer.UnitySendMessage(gameObject, methodName, json.toString());
        } catch (Exception e) {
            Log.e(TAG, "Error getting runtime info", e);
        }
    }

    // ============== Quick Checks ==============

    public static boolean isDebugEnvironment() {
        try {
            return CrashGuard.Companion.getShared().isDebugEnvironment();
        } catch (Exception e) {
            return false;
        }
    }

    public static boolean isEmulated() {
        try {
            return CrashGuard.Companion.getShared().isEmulated();
        } catch (Exception e) {
            return false;
        }
    }

    public static boolean hasRuntimeModifications() {
        try {
            return CrashGuard.Companion.getShared().hasRuntimeModifications();
        } catch (Exception e) {
            return false;
        }
    }

    public static boolean isDeveloperMode() {
        try {
            return CrashGuard.Companion.getShared().isDeveloperMode();
        } catch (Exception e) {
            return false;
        }
    }

    public static boolean isFromPlayStore() {
        try {
            return CrashGuard.Companion.getShared().isFromPlayStore();
        } catch (Exception e) {
            return false;
        }
    }

    // ============== Device Identifiers ==============

    public static String getPersistentId() {
        try {
            return CrashGuard.Companion.getShared().getPersistentId();
        } catch (Exception e) {
            return "";
        }
    }

    public static String getCrashSignature() {
        try {
            return CrashGuard.Companion.getShared().getCrashSignature();
        } catch (Exception e) {
            return null;
        }
    }

    // ============== Event Tracking ==============

    public static void recordScreenTransition(String to, String from) {
        try {
            CrashGuard.Companion.getShared().recordScreenTransition(to, from);
        } catch (Exception e) {
            Log.e(TAG, "Error recording screen transition", e);
        }
    }

    public static void recordTransactionView(String screen) {
        try {
            CrashGuard.Companion.getShared().recordTransactionView(screen != null ? screen : "payment");
        } catch (Exception e) {
            Log.e(TAG, "Error recording transaction view", e);
        }
    }

    public static void recordTransactionDismiss(String screen) {
        try {
            CrashGuard.Companion.getShared().recordTransactionDismiss(screen != null ? screen : "payment");
        } catch (Exception e) {
            Log.e(TAG, "Error recording transaction dismiss", e);
        }
    }

    public static void recordTransactionAttempt(String screen) {
        try {
            CrashGuard.Companion.getShared().recordTransactionAttempt(screen != null ? screen : "payment");
        } catch (Exception e) {
            Log.e(TAG, "Error recording transaction attempt", e);
        }
    }

    public static void recordTransactionSuccess(String screen) {
        try {
            CrashGuard.Companion.getShared().recordTransactionSuccess(screen != null ? screen : "payment");
        } catch (Exception e) {
            Log.e(TAG, "Error recording transaction success", e);
        }
    }

    public static void recordPermissionRequest(String permissionType) {
        try {
            CrashGuard.Companion.getShared().recordPermissionRequest(permissionType);
        } catch (Exception e) {
            Log.e(TAG, "Error recording permission request", e);
        }
    }

    public static void recordSettingsAccess(String screen) {
        try {
            CrashGuard.Companion.getShared().recordSettingsAccess(screen != null ? screen : "settings");
        } catch (Exception e) {
            Log.e(TAG, "Error recording settings access", e);
        }
    }

    public static void recordEvent(String name, String contextJson) {
        try {
            Map<String, Object> context = null;
            if (contextJson != null && !contextJson.isEmpty()) {
                context = new HashMap<>();
                JSONObject json = new JSONObject(contextJson);
                Iterator<String> keys = json.keys();
                while (keys.hasNext()) {
                    String key = keys.next();
                    context.put(key, json.get(key));
                }
            }
            CrashGuard.Companion.getShared().recordEvent(name, context);
        } catch (Exception e) {
            Log.e(TAG, "Error recording event", e);
        }
    }

    public static void flushEvents(final String gameObject, final String methodName) {
        try {
            CrashGuard.Companion.getShared().flushEvents(result -> {
                UnityPlayer.UnitySendMessage(gameObject, methodName, result.isSuccess() ? "true" : "false");
                return Unit.INSTANCE;
            });
        } catch (Exception e) {
            Log.e(TAG, "Error flushing events", e);
            UnityPlayer.UnitySendMessage(gameObject, methodName, "false");
        }
    }
}
