import Foundation
import CrashGuardSDK

// MARK: - Callback Types

public typealias StringCallback = @convention(c) (UnsafePointer<CChar>?) -> Void
public typealias BoolCallback = @convention(c) (Bool) -> Void

// MARK: - Helper to convert String to C string

private func withCString(_ string: String?, callback: StringCallback?) {
    guard let callback = callback else { return }
    if let string = string {
        string.withCString { ptr in
            callback(ptr)
        }
    } else {
        callback(nil)
    }
}

private func toCString(_ string: String?) -> UnsafeMutablePointer<CChar>? {
    guard let string = string else { return nil }
    let count = string.utf8.count + 1
    let result = UnsafeMutablePointer<CChar>.allocate(capacity: count)
    string.withCString { ptr in
        result.initialize(from: ptr, count: count)
    }
    return result
}

// MARK: - Initialization

@_cdecl("_CrashGuard_Start")
public func _CrashGuard_Start() {
    CrashGuard.start()
}

@_cdecl("_CrashGuard_IsInitialized")
public func _CrashGuard_IsInitialized() -> Bool {
    return true // SDK auto-initializes on shared access
}

// MARK: - Stability Analysis

@_cdecl("_CrashGuard_AnalyzeStability")
public func _CrashGuard_AnalyzeStability(_ callback: @escaping StringCallback) {
    CrashGuard.shared.analyzeStability { result in
        switch result {
        case .success(let analysis):
            let json = """
            {"id":"\(analysis.id)","signature":"\(analysis.signature)","requiresAttention":\(analysis.requiresAttention),"isActiveDiagnostics":\(analysis.isActiveDiagnostics),"isDebugBuild":\(analysis.isDebugBuild ?? false)}
            """
            json.withCString { ptr in
                callback(ptr)
            }
        case .failure(let error):
            let json = """
            {"error":"\(error.localizedDescription)"}
            """
            json.withCString { ptr in
                callback(ptr)
            }
        }
    }
}

@_cdecl("_CrashGuard_RequiresAttention")
public func _CrashGuard_RequiresAttention(_ callback: @escaping BoolCallback) {
    CrashGuard.shared.requiresAttention { result in
        switch result {
        case .success(let requires):
            callback(requires)
        case .failure:
            callback(false)
        }
    }
}

@_cdecl("_CrashGuard_RequiresAttentionCached")
public func _CrashGuard_RequiresAttentionCached() -> Bool {
    return CrashGuard.shared.lastAnalysis?.requiresAttention ?? false
}

@_cdecl("_CrashGuard_VerifySignature")
public func _CrashGuard_VerifySignature(_ signature: UnsafePointer<CChar>?, _ callback: @escaping StringCallback) {
    guard let signature = signature else {
        callback(nil)
        return
    }
    let signatureStr = String(cString: signature)

    CrashGuard.shared.verifySignature(signatureStr) { result in
        switch result {
        case .success(let verification):
            let json = """
            {"signature":"\(verification.signature)","isFlagged":\(verification.isFlagged)}
            """
            json.withCString { ptr in
                callback(ptr)
            }
        case .failure(let error):
            let json = """
            {"error":"\(error.localizedDescription)"}
            """
            json.withCString { ptr in
                callback(ptr)
            }
        }
    }
}

// MARK: - Runtime Info

@_cdecl("_CrashGuard_GetRuntimeInfo")
public func _CrashGuard_GetRuntimeInfo(_ callback: @escaping StringCallback) {
    let info = CrashGuard.shared.runtimeInfo
    let json = """
    {"isDebugEnvironment":\(info.isDebugEnvironment),"isBetaBuild":\(info.isBetaBuild),"isEmulated":\(info.isEmulated),"hasRuntimeModifications":\(info.hasRuntimeModifications),"hasDiagnosticsEnabled":\(info.hasDiagnosticsEnabled)}
    """
    json.withCString { ptr in
        callback(ptr)
    }
}

// MARK: - Quick Checks

@_cdecl("_CrashGuard_IsDebugEnvironment")
public func _CrashGuard_IsDebugEnvironment() -> Bool {
    return CrashGuard.shared.isDebugEnvironment()
}

@_cdecl("_CrashGuard_IsBetaBuild")
public func _CrashGuard_IsBetaBuild() -> Bool {
    return CrashGuard.shared.isBetaBuild()
}

@_cdecl("_CrashGuard_HasRuntimeModifications")
public func _CrashGuard_HasRuntimeModifications() -> Bool {
    return CrashGuard.shared.hasRuntimeModifications()
}

@_cdecl("_CrashGuard_IsEmulated")
public func _CrashGuard_IsEmulated() -> Bool {
    return CrashGuard.shared.runtimeInfo.isEmulated
}

// MARK: - Device Identifiers

@_cdecl("_CrashGuard_GetPersistentId")
public func _CrashGuard_GetPersistentId() -> UnsafeMutablePointer<CChar>? {
    return toCString(CrashGuard.shared.persistentId)
}

@_cdecl("_CrashGuard_GetVendorId")
public func _CrashGuard_GetVendorId() -> UnsafeMutablePointer<CChar>? {
    return toCString(CrashGuard.shared.vendorId)
}

@_cdecl("_CrashGuard_GetCrashSignature")
public func _CrashGuard_GetCrashSignature() -> UnsafeMutablePointer<CChar>? {
    return toCString(CrashGuard.shared.crashSignature)
}

// MARK: - Event Tracking

@_cdecl("_CrashGuard_RecordScreenTransition")
public func _CrashGuard_RecordScreenTransition(_ to: UnsafePointer<CChar>?, _ from: UnsafePointer<CChar>?) {
    guard let to = to else { return }
    let toStr = String(cString: to)
    let fromStr = from != nil ? String(cString: from!) : nil
    CrashGuard.shared.recordScreenTransition(to: toStr, from: fromStr)
}

@_cdecl("_CrashGuard_RecordTransactionView")
public func _CrashGuard_RecordTransactionView(_ screen: UnsafePointer<CChar>?) {
    let screenStr = screen != nil ? String(cString: screen!) : "payment"
    CrashGuard.shared.recordTransactionView(screen: screenStr)
}

@_cdecl("_CrashGuard_RecordTransactionDismiss")
public func _CrashGuard_RecordTransactionDismiss(_ screen: UnsafePointer<CChar>?) {
    let screenStr = screen != nil ? String(cString: screen!) : "payment"
    CrashGuard.shared.recordTransactionDismiss(screen: screenStr)
}

@_cdecl("_CrashGuard_RecordTransactionAttempt")
public func _CrashGuard_RecordTransactionAttempt(_ screen: UnsafePointer<CChar>?) {
    let screenStr = screen != nil ? String(cString: screen!) : "payment"
    CrashGuard.shared.recordTransactionAttempt(screen: screenStr)
}

@_cdecl("_CrashGuard_RecordTransactionSuccess")
public func _CrashGuard_RecordTransactionSuccess(_ screen: UnsafePointer<CChar>?) {
    let screenStr = screen != nil ? String(cString: screen!) : "payment"
    CrashGuard.shared.recordTransactionSuccess(screen: screenStr)
}

@_cdecl("_CrashGuard_RecordPermissionRequest")
public func _CrashGuard_RecordPermissionRequest(_ permissionType: UnsafePointer<CChar>?) {
    guard let permissionType = permissionType else { return }
    let permissionStr = String(cString: permissionType)
    CrashGuard.shared.recordPermissionRequest(permissionStr)
}

@_cdecl("_CrashGuard_RecordSettingsAccess")
public func _CrashGuard_RecordSettingsAccess(_ screen: UnsafePointer<CChar>?) {
    let screenStr = screen != nil ? String(cString: screen!) : "settings"
    CrashGuard.shared.recordSettingsAccess(screen: screenStr)
}

@_cdecl("_CrashGuard_RecordEvent")
public func _CrashGuard_RecordEvent(_ name: UnsafePointer<CChar>?, _ contextJson: UnsafePointer<CChar>?) {
    guard let name = name else { return }
    let nameStr = String(cString: name)

    var context: [String: Any]? = nil
    if let contextJson = contextJson {
        let contextStr = String(cString: contextJson)
        if let data = contextStr.data(using: .utf8),
           let json = try? JSONSerialization.jsonObject(with: data) as? [String: Any] {
            context = json
        }
    }

    CrashGuard.shared.recordEvent(nameStr, context: context)
}

@_cdecl("_CrashGuard_FlushEvents")
public func _CrashGuard_FlushEvents(_ callback: @escaping BoolCallback) {
    CrashGuard.shared.flushEvents { result in
        switch result {
        case .success:
            callback(true)
        case .failure:
            callback(false)
        }
    }
}

// MARK: - Memory Management

@_cdecl("_CrashGuard_FreeString")
public func _CrashGuard_FreeString(_ ptr: UnsafeMutablePointer<CChar>?) {
    ptr?.deallocate()
}
