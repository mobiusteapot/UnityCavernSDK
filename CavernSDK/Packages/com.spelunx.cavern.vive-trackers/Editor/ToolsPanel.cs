using UnityEngine;
using UnityEditor;

namespace SpelunxVive
{
    public class Window : EditorWindow
    {
        private Object viveManager;
        private Object viveTracker;
        private GameObject newViveManager;
        private int trackerCount;

        [MenuItem("CAVERN/Vive Trackers")]
        public static void ShowWindow()
        {
            GetWindow<Window>("Vive Tracker Tools");
        }

        void OnGUI()
        {
            GUILayout.Label("Vive Tracker", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Adds a new vive tracker to your scene and a new vive tracker manager to your scene if no tracker managers present.", MessageType.Info);

            trackerCount = GameObject.FindGameObjectsWithTag("ViveTracker").Length;
            GUILayout.Label("Current trackers in scene: " + trackerCount);

            if (GUILayout.Button("Add new Vive Tracker"))
            {
                // load from GUI input
                // objToSpawn = EditorGUILayout.ObjectField("Prefab", objToSpawn, typeof(Object), true);

                // load from path
                viveManager = (GameObject)AssetDatabase.LoadAssetAtPath("Packages/com.spelunx.cavern.vive-trackers/Prefabs/ViveTrackerManager.prefab", typeof(GameObject));
                viveTracker = (GameObject)AssetDatabase.LoadAssetAtPath("Packages/com.spelunx.cavern.vive-trackers/Prefabs/ViveTracker.prefab", typeof(GameObject));
                // Debug.Log("vive setup: " + viveManager.name + " " + viveTracker.name);
                // Debug.Log("Got " + objToSpawn.name + objToSpawn.GetType());


                // check if vive tracker manager is already present
                if (GameObject.FindGameObjectsWithTag("ViveManager").Length == 0)
                {
                    newViveManager = (GameObject)PrefabUtility.InstantiatePrefab(viveManager as GameObject);

                    // set vive manager to be in the CAVERN setup folder in the scene hierarchy
                    GameObject cavernSetup = GameObject.Find("CavernSetup");
                    if (cavernSetup != null)
                    {
                        newViveManager.GetComponent<Transform>().parent = cavernSetup.transform;
                    }
                }

                // instantiate a new vive tracker
                PrefabUtility.InstantiatePrefab(viveTracker as GameObject);
            }
        }
    }

}


