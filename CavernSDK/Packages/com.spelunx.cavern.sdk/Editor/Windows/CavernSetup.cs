using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Spelunx
{
    public class CavernSetup : EditorWindow
    {
    private Object cavernSetup;
    private Object viveManager;
    private Object viveTracker;
    private GameObject newCavernSetup;
    private GameObject newViveManager;
    private GameObject newViveTracker;
    private AudioConfiguration audioConfigs;

    [MenuItem("Window/UIToolkit/Cavern Setup")]


    public static void ShowExample()
    {
        CavernSetup wnd = GetWindow<CavernSetup>();
        wnd.titleContent = new GUIContent("Cavern Setup");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        VisualElement setupLabel = new Label("Setup tools for a CAVERN Project");
        root.Add(setupLabel);

        VisualElement canvasLabel = new Label("Round CAVERN Cavnas");
        root.Add(canvasLabel);

        
    }
    }
}
