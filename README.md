
# DxOPatcher

Simple application for patching license activation against DxO PhotoLab. This application patches the following features from the Affinity Persona assembly,

- License check (entitlement to run)
- Eula acceptance
- Crash report uploading
- Analytics upload

## Usage

```
Description:
  Simple application for patching license activation against DxO PhotoLab.

Usage:
  DxOPatcher [options]

Options:
  --input <input> (REQUIRED)  Target DxO PhotoLab directory (i.e., path containing DxO.PhotoLab.exe).
  --verbose                   Enable verbose logging.
  --keep                      Backup original assembly.
  --version                   Show version information
  -?, -h, --help              Show help and usage information
```

## Exmaple

```
> dotnet run -- --input "C:\Program Files\DxO\DxO PhotoLab 8" --keep --verbose
Backed up original assembly.
Located System.Boolean DxO.PhotoLab.Activation.Feature.AllFeatureBase`2::get_IsActivated(), patching with "return true".
Located System.Boolean DxO.PhotoLab.Activation.Feature.AllFeatureBase`2::get_HasAnyLicense(), patching with "return
true".
Located System.Boolean DxO.PhotoLab.Activation.Feature.AllFeatureBase`2::get_IsValid(), patching with "return true".
Located System.Boolean DxO.PhotoLab.Activation.Feature.AllFeatureBase`2::get_IsDemo(), patching with "return false".
Located System.Boolean DxO.PhotoLab.Activation.Feature.AllFeatureBase`2::get_IsExpired(), patching with "return false".
Located System.Boolean DxO.PhotoLab.Activation.Feature.AllFeatureBase`2::get_IsTemporary(), patching with "return false"
.
Located System.UInt32 DxO.PhotoLab.Activation.Feature.AllFeatureBase`2::get_RemainingDays(), patching with "99".
Located System.Int32 DxO.PhotoLab.Activation.Feature.AllFeatureBase`2::RemainingOfflineDays(System.Int16&), patching
with "99".
Located System.Boolean DxO.PhotoLab.Activation.Feature.FeatureBase`2::get_IsActivated(), patching with "return true".
Located System.Boolean DxO.PhotoLab.Activation.Feature.FeatureBase`2::get_HasAnyLicense(), patching with "return true".
Located System.Boolean DxO.PhotoLab.Activation.Feature.FeatureBase`2::get_IsValid(), patching with "return true".
Located System.Boolean DxO.PhotoLab.Activation.Feature.FeatureBase`2::get_IsDemo(), patching with "return false".
Located System.Boolean DxO.PhotoLab.Activation.Feature.FeatureBase`2::get_IsTemporary(), patching with "return false".
Located System.Boolean DxO.PhotoLab.Activation.Feature.FeatureBase`2::get_IsExpired(), patching with "return false".
Located System.UInt32 DxO.PhotoLab.Activation.Feature.FeatureBase`2::get_RemainingDays(), patching with "99".
Located System.Int32 DxO.PhotoLab.Activation.Feature.FeatureBase`2::RemainingOfflineDays(System.Int16&), patching with
"99".
Located System.Boolean DxO.PhotoLab.Activation.Feature.PhotoLabAllFeature::get_IsActivated(), patching with "return
true".
Located System.String
DxO.PhotoLab.Activation.FeatureSplashContents.FeatureSplashContentBase`1::FormatRemainingDays(System.UInt32), patching
with "99".
Located System.Boolean DxO.PhotoLab.Activation.FeatureSplashContents.PhotoLabFeatureSplashContent::get_IsValid(),
patching with "return true".
┌─Patched──────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│                                                                                                                      │
│ - System.Boolean DxO.PhotoLab.Activation.Feature.AllFeatureBase`2::get_IsActivated()                                 │
│ - System.Boolean DxO.PhotoLab.Activation.Feature.AllFeatureBase`2::get_HasAnyLicense()                               │
│ - System.Boolean DxO.PhotoLab.Activation.Feature.AllFeatureBase`2::get_IsValid()                                     │
│ - System.Boolean DxO.PhotoLab.Activation.Feature.AllFeatureBase`2::get_IsDemo()                                      │
│ - System.Boolean DxO.PhotoLab.Activation.Feature.AllFeatureBase`2::get_IsExpired()                                   │
│ - System.Boolean DxO.PhotoLab.Activation.Feature.AllFeatureBase`2::get_IsTemporary()                                 │
│ - System.UInt32 DxO.PhotoLab.Activation.Feature.AllFeatureBase`2::get_RemainingDays()                                │
│ - System.Int32 DxO.PhotoLab.Activation.Feature.AllFeatureBase`2::RemainingOfflineDays(System.Int16&)                 │
│ - System.Boolean DxO.PhotoLab.Activation.Feature.FeatureBase`2::get_IsActivated()                                    │
│ - System.Boolean DxO.PhotoLab.Activation.Feature.FeatureBase`2::get_HasAnyLicense()                                  │
│ - System.Boolean DxO.PhotoLab.Activation.Feature.FeatureBase`2::get_IsValid()                                        │
│ - System.Boolean DxO.PhotoLab.Activation.Feature.FeatureBase`2::get_IsDemo()                                         │
│ - System.Boolean DxO.PhotoLab.Activation.Feature.FeatureBase`2::get_IsTemporary()                                    │
│ - System.Boolean DxO.PhotoLab.Activation.Feature.FeatureBase`2::get_IsExpired()                                      │
│ - System.UInt32 DxO.PhotoLab.Activation.Feature.FeatureBase`2::get_RemainingDays()                                   │
│ - System.Int32 DxO.PhotoLab.Activation.Feature.FeatureBase`2::RemainingOfflineDays(System.Int16&)                    │
│ - System.Boolean DxO.PhotoLab.Activation.Feature.PhotoLabAllFeature::get_IsActivated()                               │
│ - System.String                                                                                                      │
│ DxO.PhotoLab.Activation.FeatureSplashContents.FeatureSplashContentBase`1::FormatRemainingDays(System.UInt32)         │
│ - System.Boolean DxO.PhotoLab.Activation.FeatureSplashContents.PhotoLabFeatureSplashContent::get_IsValid()           │
│                                                                                                                      │
│                                                                                                                      │
└──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
Assembly saved as C:\Program Files\DxO\DxO PhotoLab 8\DxO.PhotoLab.Activation.dll with file size 155.5 KB.
Backed up original assembly.
Located System.Boolean DxOActivation.Activation::get_IsValid(), patching with "return true".
Located System.Boolean DxOActivation.Activation::get_IsActivated(), patching with "return true".
Located System.Boolean DxOActivation.Activation::get_IsDemo(), patching with "return false".
Located System.Boolean DxOActivation.Activation::get_IsExpired(), patching with "return false".
Located System.Boolean DxOActivation.Activation::get_IsTemporary(), patching with "return false".
Located DxOActivation.ActivationResult DxOActivation.Activation::Check(System.Boolean,System.Int16&), patching with
"return false".
Located System.Int32 DxOActivation.Activation::get_RemainingDays(), patching with "99".
Located System.Int32 DxOActivation.Activation::RemainingOfflineDays(System.Int16&), patching with "99".
┌─Patched────────────────────────────────────────────────────────────────────────────────────────┐
│                                                                                                │
│ - System.Boolean DxOActivation.Activation::get_IsValid()                                       │
│ - System.Boolean DxOActivation.Activation::get_IsActivated()                                   │
│ - System.Boolean DxOActivation.Activation::get_IsDemo()                                        │
│ - System.Boolean DxOActivation.Activation::get_IsExpired()                                     │
│ - System.Boolean DxOActivation.Activation::get_IsTemporary()                                   │
│ - DxOActivation.ActivationResult DxOActivation.Activation::Check(System.Boolean,System.Int16&) │
│ - System.Int32 DxOActivation.Activation::get_RemainingDays()                                   │
│ - System.Int32 DxOActivation.Activation::RemainingOfflineDays(System.Int16&)                   │
│                                                                                                │
│                                                                                                │
└────────────────────────────────────────────────────────────────────────────────────────────────┘
Assembly saved as C:\Program Files\DxO\DxO PhotoLab 8\DxO.PhotoLab.Activation.Interop.dll with file size 7.57 MB.
```

## License

[MIT](https://choosealicense.com/licenses/mit/)
