using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
// Forked from https://github.com/JorGra/JG-UnityEditor-GameViewFullscreen
/// <summary>
/// When enabled or activated, tries to run the game view full screen by rendering the game view to a new window that is the size of the monitor, and hiding the toolbar.
/// Does not use the same API as a native fullscreen build, so performance will not be the same as "exclusive" fullscreen
/// </summary>
public static class FullscreenGameView
{
    static readonly Type GameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
    static readonly MethodInfo SetSizeProperty = GameViewType.GetMethod("SizeSelectionCallback", BindingFlags.Instance | BindingFlags.NonPublic);

    static EditorWindow instance;

    [MenuItem("Window/General/Game (Fullscreen) _F11", priority = 2)]
    public static void Toggle()
    {
        if(!EditorApplication.isPlaying)
        {
            Debug.LogWarning("You can only enter fullscreen mode while the game is running.");
            return;
        }

        if(instance == null)
        {
            EnterFullscreen();
        }
        else
        {
            ExitFullscreen();
        }
    }

    private static void EnterFullscreen()
    {
        if(GameViewType == null)
        {
            Debug.LogError("GameView type not found.");
            return;
        }

        instance = ScriptableObject.CreateInstance(GameViewType) as EditorWindow;

        int monitorWidth = (int)(Screen.currentResolution.width / EditorGUIUtility.pixelsPerPoint);
        int monitorHeight = (int)(Screen.currentResolution.height / EditorGUIUtility.pixelsPerPoint);

        var gameViewSizesInstance = GetGameViewSizesInstance();
        if(SetSizeProperty != null && gameViewSizesInstance != null)
        {
            int sizeIndex = FindResolutionSizeIndex(monitorWidth, monitorHeight, gameViewSizesInstance);
            SetSizeProperty.Invoke(instance, new object[] { sizeIndex, null });
        }

        var desktopResolution = new Vector2(monitorWidth, monitorHeight);
        var fullscreenRect = new Rect(Vector2.zero, desktopResolution);

        instance.ShowPopup();
        instance.position = fullscreenRect;
        instance.Focus();

        EditorApplication.delayCall += ApplyToolbarPatches;
    }

    private static void ExitFullscreen()
    {
        if(instance != null)
        {
            instance.Close();
            instance = null;

            EditorApplication.delayCall += ApplyToolbarPatches;
        }
    }


    public static void SetFullscreen(bool fullscreen)
    {
        if(instance == null && fullscreen)
        {
            EnterFullscreen();
        }
        else if(instance != null && !fullscreen)
        {
            ExitFullscreen();
        }
    }

    private static void ApplyToolbarPatches()
    {
        // Apply the patches here, now that Unity had a frame to finalize the view
        GameViewToolbarHiderAlternative.ToggleAlternateToolbarRemoval();
    }
    private static object GetGameViewSizesInstance()
    {
        var sizesType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameViewSizes");
        var singleType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
        var instanceProp = singleType.GetProperty("instance");
        return instanceProp.GetValue(null, null);
    }

    private static int FindResolutionSizeIndex(int width, int height, object gameViewSizesInstance)
    {
        var groupType = gameViewSizesInstance.GetType().GetMethod("GetGroup");
        var currentGroup = groupType.Invoke(gameViewSizesInstance, new object[] { (int)GameViewType.GetMethod("GetCurrentGameViewSizeGroupType").Invoke(instance, null) });

        var getBuiltinCount = currentGroup.GetType().GetMethod("GetBuiltinCount");
        var getCustomCount = currentGroup.GetType().GetMethod("GetCustomCount");
        var getGameViewSize = currentGroup.GetType().GetMethod("GetGameViewSize");

        int totalSizes = (int)getBuiltinCount.Invoke(currentGroup, null) + (int)getCustomCount.Invoke(currentGroup, null);

        for (int i = 0; i < totalSizes; i++)
        {
            var size = getGameViewSize.Invoke(currentGroup, new object[] { i });
            var widthProp = size.GetType().GetProperty("width");
            var heightProp = size.GetType().GetProperty("height");

            int w = (int)widthProp.GetValue(size, null);
            int h = (int)heightProp.GetValue(size, null);

            if (w == width && h == height)
            {
                return i;
            }
        }

        Debug.LogWarning("Resolution not found. Defaulting to index 0.");
        return 0;
    }

    [MenuItem("Window/LayoutShortcuts/Default", false, 2)]
    static void DefaultLayout()
    {
        EditorApplication.ExecuteMenuItem("Window/Layouts/Default");
    }
}