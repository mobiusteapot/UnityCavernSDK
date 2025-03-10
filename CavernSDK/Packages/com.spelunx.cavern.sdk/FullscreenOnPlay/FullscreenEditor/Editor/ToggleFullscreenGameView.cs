using UnityEditor;


public class ToggleFullscreenGameView : Editor
{
    public const string MenuPathPrefix = "CAVERN";
    public const string IsEnabledBoolPath = AutoActivateFullscreenPreview.IsFullscreenPreviewEnabledKey;
    private const string FullMenuPath = MenuPathPrefix + "/" + IsEnabledBoolPath;

    [MenuItem(FullMenuPath)]
    public static void ToggleIsEnabled()
    {
        EditorPrefs.SetBool(IsEnabledBoolPath, !EditorPrefs.GetBool(IsEnabledBoolPath, false));
        Menu.SetChecked(FullMenuPath, EditorPrefs.GetBool(IsEnabledBoolPath, false));
    }

    [MenuItem(FullMenuPath, true)]
    public static bool ToggleIsEnabledValidate()
    {
        Menu.SetChecked(FullMenuPath, EditorPrefs.GetBool(IsEnabledBoolPath, false));
        return true;
    }
}
