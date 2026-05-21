# CrashGuard SDK for Unity

Lightweight crash analytics and stability monitoring SDK for Unity. iOS + Android.

## Installation

### Via Package Manager (Git URL)

1. Open **Window > Package Manager**
2. Click **+** → **Add package from git URL**
3. Enter: `https://github.com/zey-win/CrashGuardSDK-Unity.git`
4. Click **Add**

### Auto-install via ZeyWin Ads SDK

CrashGuard is auto-installed when the [ZeyWin Ads SDK](https://github.com/zey-win/ZeyWinAdsSDK-Unity) is added to a project — no manual step required.

### Manual Installation

Copy the contents of this repo into your project's `Packages/com.crashguard.sdk/` folder, or import `CrashGuard-1.0.1.unitypackage`.

## Usage

```csharp
using CrashGuard;

void Start()
{
    CrashGuard.Start();

    CrashGuard.AnalyzeStability(result =>
    {
        if (result.requiresAttention)
        {
            // Environment may be unstable
        }
    });
}
```

See `Runtime/CrashGuard.cs` for the full API.

## Platforms

- iOS (arm64 device + simulator) via `CrashGuardSDK.xcframework`
- Android via `crashguard.aar`
- Unity Editor: mocked

## iOS post-processing

`CrashGuardPostProcessor` runs at build time and:

- Adds `Security`, `UIKit`, `Foundation` frameworks
- Sets `SWIFT_VERSION=5.0`, disables Bitcode
- Embeds Swift libs, sets rpath
- Copies `PrivacyInfo.xcprivacy` into the Xcode project

## Release verification

- Unity Editor compile: verified through the ZeyWin sample project.
- Android target compile: verified.
- Android APK smoke build: verified.
- iOS target compile: verified.
- iOS Xcode export smoke build: verified.
- Git URL install hash: `1d73be279a37c2576c3d0395e4315cf2de96429b`.
