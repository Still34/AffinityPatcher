
# AffinityPatcher

Simple application for patching license activation amongst Affinity v2.x/v1.x and DxO products. 

## Patches

### Affinity 
- Licensecheck (entitlement to run)
- Eula acceptance
- Crash report uploading
- Analytics upload

### DxO
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

- You can get products from all supported companies on their website - whose links are not provided here. Do your own homework.

```
Description:
  Universal application patcher for Affinity v2.x/v1.x products and DxO PhotoLab.

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

