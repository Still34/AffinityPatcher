
# AffinityPatcher

Simple application for patching license activation amongst Affinity v3.x/v2.x.

> [!NOTE]  
> v1.x is currently unsupported. Support may be added in the future. See [#14](https://github.com/Still34/AffinityPatcher/issues/14).

## Supported Platforms

- Windows
- macOS and Linux are not supported.
    - A Linux build for the patcher is still provided in case you wish to patch the Windows version and run the product under WINE.

## Patches

### Affinity

- License check (entitlement to run)
- Eula acceptance
- Crash report uploading
- Analytics upload
    - Note that the patches do not eliminate all analytics upload; only optional opt-in ones are denied. See [#11](https://github.com/Still34/AffinityPatcher/issues/11).
- AppMode (unsure what this does yet; patched to `Ultimate`)
- Skip linking Affinity ID / Canva account
    - This means features that require Canva Premium (ones with a crown logo next to the feature) will not work.

### DxO

> [!NOTE]  
> The implementation is currently broken. See [#9](https://github.com/Still34/AffinityPatcher/issues/9).

- Features
    - IsActivated
    - IsTemporary
    - IsDemo
    - IsValid
    - RemainingOfflineDays
    - RemainingDays
- Activation
    - IsElite

## Usage

1. Obtain the product installer and install it as usual. The links are not provided here. Do your own homework.
1. Make sure the desired application is not running.
1. Extract the patcher to your directory of choice.
1. Open your favorite shell (`cmd`, `powershell`, `pwsh`, etc.) and navigate to the directory; you may need to run your shell as Administrator if your product is installed under `Program Files`.
1. Execute the patcher with arguments that targets the directory containing the product (e.g., `./AffinityPatcher.exe --input C:\Affinity\Photos --keep --verbose`)

```
Description:
  Universal application patcher for Affinity v3.x/v2.x/v1.x products and DxO PhotoLab.

Usage:
  AffinityPatcher [options]

Options:
  --input <input> (REQUIRED)  Target application directory (i.e., path containing the main executable).
  --mode <Affinity|DxO>       Select which application to patch. [default: Affinity]
  --verbose                   Enable verbose logging.
  --keep                      Backup original assembly.
  --version                   Show version information
  -?, -h, --help              Show help and usage information
```

## Is this safe?

All the patcher does is patch the methods to either true or false - that's it. You can manually fix the assembly based on the function names found in the code yourself. This just makes you and I's life easeier.

## License

[MIT](https://choosealicense.com/licenses/mit/)
