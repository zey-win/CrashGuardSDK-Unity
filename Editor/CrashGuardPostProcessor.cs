#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace CrashGuard.Editor
{
    /// <summary>
    /// Post-process iOS builds to configure required frameworks and settings
    /// </summary>
    public static class CrashGuardPostProcessor
    {
        [PostProcessBuild(100)]
        public static void OnPostProcessBuild(BuildTarget target, string path)
        {
            if (target != BuildTarget.iOS) return;

            // Modify Xcode project
            var projPath = PBXProject.GetPBXProjectPath(path);
            var project = new PBXProject();
            project.ReadFromFile(projPath);

#if UNITY_2019_3_OR_NEWER
            var targetGuid = project.GetUnityMainTargetGuid();
            var frameworkGuid = project.GetUnityFrameworkTargetGuid();
#else
            var targetGuid = project.TargetGuidByName(PBXProject.GetUnityTargetName());
            var frameworkGuid = targetGuid;
#endif

            // Add required system frameworks to UnityFramework target (where Swift code lives)
            project.AddFrameworkToProject(frameworkGuid, "Security.framework", false);
            project.AddFrameworkToProject(frameworkGuid, "UIKit.framework", false);
            project.AddFrameworkToProject(frameworkGuid, "Foundation.framework", false);

            // Enable Swift support (required for CrashGuardBridge.swift)
            project.SetBuildProperty(targetGuid, "SWIFT_VERSION", "5.0");
            project.SetBuildProperty(frameworkGuid, "SWIFT_VERSION", "5.0");

            // Add system framework search paths
            project.AddBuildProperty(targetGuid, "FRAMEWORK_SEARCH_PATHS", "$(inherited)");
            project.AddBuildProperty(targetGuid, "FRAMEWORK_SEARCH_PATHS", "$(SDKROOT)/System/Library/Frameworks");
            project.AddBuildProperty(frameworkGuid, "FRAMEWORK_SEARCH_PATHS", "$(inherited)");
            project.AddBuildProperty(frameworkGuid, "FRAMEWORK_SEARCH_PATHS", "$(SDKROOT)/System/Library/Frameworks");

            // Disable Bitcode (deprecated in Xcode 14+)
            project.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");
            project.SetBuildProperty(frameworkGuid, "ENABLE_BITCODE", "NO");

            // Set Swift bridging header search paths
            project.SetBuildProperty(targetGuid, "SWIFT_OBJC_BRIDGING_HEADER", "");
            project.AddBuildProperty(targetGuid, "LD_RUNPATH_SEARCH_PATHS", "@executable_path/Frameworks");
            project.AddBuildProperty(frameworkGuid, "LD_RUNPATH_SEARCH_PATHS", "@executable_path/Frameworks");

            // Embed frameworks
            project.SetBuildProperty(targetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");

            // Add Privacy Manifest to Xcode project
            AddPrivacyManifest(project, targetGuid, path);

            project.WriteToFile(projPath);

            // Modify Info.plist if needed
            var plistPath = Path.Combine(path, "Info.plist");
            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);

            // Add any required Info.plist keys here if needed
            // Example: plist.root.SetString("NSUserTrackingUsageDescription", "...");

            plist.WriteToFile(plistPath);

            UnityEngine.Debug.Log("[CrashGuard] iOS post-process completed");
        }

        private static void AddPrivacyManifest(PBXProject project, string targetGuid, string buildPath)
        {
            // Find PrivacyInfo.xcprivacy in the Plugins/iOS folder
            var pluginsPath = Path.Combine(UnityEngine.Application.dataPath, "Plugins", "iOS");

            // Also check in Packages (for UPM installations)
            string privacyManifestSource = null;

            var packagePath = "Packages/com.crashguard.sdk/Plugins/iOS/PrivacyInfo.xcprivacy";
            if (File.Exists(Path.Combine(UnityEngine.Application.dataPath, "..", packagePath)))
            {
                privacyManifestSource = Path.Combine(UnityEngine.Application.dataPath, "..", packagePath);
            }
            else if (Directory.Exists(pluginsPath))
            {
                var assetsPrivacyPath = Path.Combine(pluginsPath, "PrivacyInfo.xcprivacy");
                if (File.Exists(assetsPrivacyPath))
                {
                    privacyManifestSource = assetsPrivacyPath;
                }
            }

            if (string.IsNullOrEmpty(privacyManifestSource) || !File.Exists(privacyManifestSource))
            {
                UnityEngine.Debug.LogWarning("[CrashGuard] PrivacyInfo.xcprivacy not found, skipping...");
                return;
            }

            // Copy to Xcode project root
            var privacyManifestDest = Path.Combine(buildPath, "PrivacyInfo.xcprivacy");
            File.Copy(privacyManifestSource, privacyManifestDest, true);

            // Add to Xcode project
            var fileGuid = project.AddFile(privacyManifestDest, "PrivacyInfo.xcprivacy", PBXSourceTree.Source);
            project.AddFileToBuild(targetGuid, fileGuid);

            UnityEngine.Debug.Log("[CrashGuard] Added PrivacyInfo.xcprivacy to Xcode project");
        }
    }
}
#endif
