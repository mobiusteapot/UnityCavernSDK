# Fullscreen Game View

This package allows toggling the Unity Game View to fullscreen mode by pressing **F11** during Play mode.

## How It Works

The package uses reflection and [Harmony](https://github.com/pardeike/Harmony) to modify internal Unity Editor behavior.  
**Note:** Due to reliance on internal APIs, future Unity updates may break compatibility.  
This solution has been tested and confirmed working with **Unity 2022.3**.

## Known Issues

- **Monitor Limitation:** Fullscreen currently works reliably only on the monitor on which Unity was initially launched.
- **Startup Error:** An error might appear upon opening Unity if Burst is enabled. This error is harmless and can be safely ignored.
- **Performance:** In some configurations, the Game View may render twice, potentially impacting performance.

## Installation

1. Open Unity’s Package Manager.
2. Click the **+** button.
3. Select **Add package from git URL...**
4. Paste the URL of this repository.

Once installed, press **F12** during Play mode to toggle fullscreen on the Game View.

## Support

For issues or suggestions, please open an issue in the repository.
