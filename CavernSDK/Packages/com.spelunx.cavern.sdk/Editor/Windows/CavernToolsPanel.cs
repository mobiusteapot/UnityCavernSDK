using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;


namespace Spelunx
{
    public class CavernToolsPanel : EditorWindow
    {
        const float PADDING = 20.0f;

        [MenuItem("CAVERN/CAVERN Tools", false, 100)]
        public static void ShowWindow()
        {
            GetWindow<CavernToolsPanel>("CAVERN Tools");
        }

        private void OnGUI()
        {
            //===== CAVERN Setup =====
            GUILayout.Label("CAVERN Setup", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Sets up scene for CAVERN development. Replaces the default Unity camera with the CAVERN camera rig. Defaults audio speaker mode to 7.1 surround sound.", MessageType.Info);
            if (GUILayout.Button("Add new CAVERN setup"))
            {
                // adds cavern setup prefab to scene
                GameObject cavernSetupPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Packages/com.spelunx.cavern.sdk/Prefabs/CavernSetup.prefab", typeof(GameObject));
                GameObject cavernSetupInstance = (GameObject)PrefabUtility.InstantiatePrefab(cavernSetupPrefab as GameObject);

                // load in the debug keys
                // newCavernSetup.GetComponent<DebugManager>().AddKeyManager(new BuiltInKeys());

                // sets speaker mode to 7.1 surround
                AudioConfiguration audioConfigs = AudioSettings.GetConfiguration();
                audioConfigs.speakerMode = AudioSpeakerMode.Mode7point1;
                AudioSettings.Reset(audioConfigs);

                // removes any default main cameras in scene (but preserves any cameras not tagged as MainCamera)
                GameObject mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
                if (mainCamera != null)
                {
                    Undo.DestroyObjectImmediate(GameObject.FindGameObjectWithTag("MainCamera"));
                }
                bool isDirty = EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }

            // We do not want to show any of the other options unless there is a CavernRenderer in the scene.
            CavernRenderer cavernRenderer = FindFirstObjectByType<CavernRenderer>();
            if (cavernRenderer == null) return;

            //===== Round UI =====
            GUILayout.Space(PADDING);
            GUILayout.Label("Round CAVERN UI Setup", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Creates a curved world space UI that matches the curvature of the CAVERN. This is used to wrap 2D visuals around the CAVERN.", MessageType.Info);
            if (GUILayout.Button("Add new RoundUI setup"))
            {
                GameObject cavernUIPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Packages/com.spelunx.cavern.sdk/Prefabs/CavernUI.prefab", typeof(GameObject));
                GameObject cavernUIInstance = (GameObject)PrefabUtility.InstantiatePrefab(cavernUIPrefab as GameObject);

                GameObject roundCavernMeshRendererPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Packages/com.spelunx.cavern.sdk/Prefabs/RoundCavernMeshRenderer.prefab", typeof(GameObject));
                GameObject roundCavernMeshRendererInstance = (GameObject)PrefabUtility.InstantiatePrefab(roundCavernMeshRendererPrefab as GameObject);

                WorldSpaceMeshCanvas meshCanvas = roundCavernMeshRendererInstance.GetComponent<WorldSpaceMeshCanvas>();
                meshCanvas.setCavernRenderer(cavernRenderer);

                // Do it this way instead. This way there's no need to worry about the child being reordered or whatever.
                meshCanvas.transform.parent = cavernRenderer.transform;
                meshCanvas.transform.localPosition = Vector3.zero;
                meshCanvas.transform.localRotation = Quaternion.identity;

                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
        }
    }

}