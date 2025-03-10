# Fullscreen Game View

This package allows toggling the Unity Game View to fullscreen mode by pressing **F11** during Play mode.

## How It Works

The package uses reflection and [Harmony](https://github.com/pardeike/Harmony) to modify internal Unity Editor behavior.  
**Note:** Due to reliance on internal APIs, future Unity updates may break compatibility.  
This solution has been tested and confirmed working with **Unity 6000.0.34f**.

## Known Issues

- **Monitor Limitation:** Fullscreen currently works reliably only on the monitor on which Unity was initially launched.
- **Startup Error:** An error might appear upon opening Unity if Burst is enabled. This error is harmless and can be safely ignored.
- **Performance:** The game view window will render twice.

## Installation

1. Open Unity’s Package Manager.
2. Click the **+** button.
3. Select **Add package from git URL...**
4. Paste the URL of this repository.

Once installed, go to "Tools/FullScreenGameViewEnabled" to toggle if the full screen view should be used on play.

You will need to hide the task bar on most configurations.

You can press **F11** at any time to toggle the fullscreen view.
