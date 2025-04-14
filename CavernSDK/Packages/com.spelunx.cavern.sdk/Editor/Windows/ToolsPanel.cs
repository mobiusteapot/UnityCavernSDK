using UnityEngine;
using UnityEditor;
using UnityEditor.SearchService;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;


namespace Spelunx
{
    public class Window : EditorWindow
    {
        private Object cavernSetup;
        private GameObject newCavernSetup;
        private Object cavernUI;
        private Object roundRenderer;

        private GameObject newCavernUI;
        private GameObject newRoundRenderer;
        private AudioConfiguration audioConfigs;
        private UnityEngine.SceneManagement.Scene scene;

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

                // sets speaker mode to 7.1 surround
                audioConfigs = AudioSettings.GetConfiguration();
                audioConfigs.speakerMode = AudioSpeakerMode.Mode7point1;
                AudioSettings.Reset(audioConfigs);

                // removes any default main cameras in scene (but preserves any cameras not tagged as MainCamera)
                GameObject mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
                if (mainCamera != null)
                {
                    Undo.DestroyObjectImmediate(GameObject.FindGameObjectWithTag("MainCamera"));
                }
                scene = SceneManager.GetActiveScene();
                bool isDirty = EditorSceneManager.MarkSceneDirty(scene);
                Debug.Log(scene.name + isDirty);
            }

            GUILayout.Label("Round CAVERN UI Setup", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Creates a curved world space UI that matches the curvature of the CAVERN. This is used to wrap 2D visuals around the CAVERN.", MessageType.Info);
            if (GUILayout.Button("Add new RoundUI setup"))
            {
                Debug.Log("round UI clicked");
                cavernUI = (GameObject)AssetDatabase.LoadAssetAtPath("Packages/com.spelunx.cavern.sdk/Runtime/Prefabs/CavernUI.prefab", typeof(GameObject));
                newCavernUI = (GameObject)PrefabUtility.InstantiatePrefab(cavernUI as GameObject);

                roundRenderer = (GameObject)AssetDatabase.LoadAssetAtPath("Packages/com.spelunx.cavern.sdk/Runtime/Prefabs/RoundCavernMeshRenderer.prefab", typeof(GameObject));
                newRoundRenderer = (GameObject)PrefabUtility.InstantiatePrefab(roundRenderer as GameObject);

                WorldSpaceMeshCanvas meshCanvas = newRoundRenderer.GetComponent<WorldSpaceMeshCanvas>();
                meshCanvas.setCavernRenderer(GameObject.Find("CavernCamera").GetComponent<CavernRenderer>());

                scene = SceneManager.GetActiveScene();
                bool isDirty = EditorSceneManager.MarkSceneDirty(scene);
                Debug.Log(isDirty);
            }
        }
    }

}


