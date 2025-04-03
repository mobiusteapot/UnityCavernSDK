using UnityEngine;
using UnityEditor;
using UnityEditor.SearchService;
using UnityEngine.SceneManagement;

namespace Spelunx
{
    public class Window : EditorWindow
    {
        private Object cavernSetup;
        private Object viveManager;
        private Object viveTracker;
        private GameObject newCavernSetup;
        private GameObject newViveManager;
        private GameObject newViveTracker;
        private AudioConfiguration audioConfigs;

        [MenuItem("CAVERN/Tools")]
        public static void ShowWindow()
        {
            GetWindow<Window>("CAVERN Tools and Setup");
        }

        void OnGUI()
        {
            GUILayout.Label("CAVERN Setup", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Sets up scene for CAVERN development. Replaces the default Unity camera with the CAVERN camera rig. Defaults audio speaker mode to 7.1 surround sound.", MessageType.Info);
            // GUILayout.Label("Replaces the default Unity camera with the CAVERN camera rig in your scene");
            // GUILayout.Label("and creates a CAVERN setup game object to hold additional CAVERN objects.");
            if (GUILayout.Button("Add new CAVERN setup"))
            {
                // adds cavern setup prefab to scene
                cavernSetup = (GameObject)AssetDatabase.LoadAssetAtPath("Packages/com.spelunx.cavern.sdk/Runtime/Prefabs/CavernSetup.prefab", typeof(GameObject));
                newCavernSetup = (GameObject)PrefabUtility.InstantiatePrefab(cavernSetup as GameObject);

                // load in the debug keys
                // newCavernSetup.GetComponent<DebugManager>().AddKeyManager(new BuiltInKeys());

                // sets speaker mode to 5.1 surround
                audioConfigs = AudioSettings.GetConfiguration();
                audioConfigs.speakerMode = AudioSpeakerMode.Mode7point1;
                AudioSettings.Reset(audioConfigs);

                // removes any default main cameras in scene (but preserves any cameras not tagged as MainCamera)
                GameObject mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
                if (mainCamera != null)
                {
                    Undo.DestroyObjectImmediate(GameObject.FindGameObjectWithTag("MainCamera"));
                }
            }
        }
    }

}


