using UnityEngine;
using UnityEditor;
using System;

namespace Spelunx.Vive
{
    public class ToolsPanel : EditorWindow
    {
        private UnityEngine.Object viveManager;
        private UnityEngine.Object viveTracker;
        private GameObject newViveManager;
        private int trackerCount;

        [MenuItem("CAVERN/Vive Trackers")]
        public static void ShowWindow()
        {
            GetWindow<ToolsPanel>("Vive Tracker Tools");
        }

        void OnGUI()
        {
            // === add Vive Tracker and Manager ===
            GUILayout.Label("Vive Tracker", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Adds a new vive tracker to your scene and a new vive tracker manager to your scene if no tracker managers present.", MessageType.Info);

            TagAdder.AddTag("ViveManager");
            TagAdder.AddTag("ViveTracker");

            trackerCount = GameObject.FindGameObjectsWithTag("ViveTracker").Length;
            GUILayout.Label("Current trackers in scene: " + trackerCount);

            // === Add Vive Tracker to Scene ===
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
                    // load in the debug keys
                    cavernSetup.GetComponent<DebugManager>().AddKeyManager(new ViveDebugKeys());
                }

                // instantiate a new vive tracker
                PrefabUtility.InstantiatePrefab(viveTracker as GameObject);
            }

            //=== interaction building blocks ===
            GUILayout.Space(20);
            GUILayout.Label("Tracker Interaction Building Blocks", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("A collection of interactions using Vive Trackers.", MessageType.Info);

            GUILayout.Space(10);
            GUILayout.Label("Follow Motion");
            EditorGUILayout.HelpBox("Object matches the source object's position. Contains an adjustable offset to allow following source object from a set distance away.", MessageType.Info);
            if (GUILayout.Button("Add Follow Motion"))
            {
                AddInteraction(typeof(FollowMotion));
                // Debug.Log("follow motion button pressed");
            }

            GUILayout.Space(10);
            GUILayout.Label("Mirror Position in CAVERN");
            EditorGUILayout.HelpBox("Object mirrors the source object's position with the CAVERN screen functioning as the mirror surface it reflects across. Contains a deadzone where the reflection doesn't change to prevent spinning with an adjustable radius.", MessageType.Info);
            if (GUILayout.Button("Add CAVERN Mirror"))
            {
                AddInteraction(typeof(CavernMirrorMotion));
                // Debug.Log("CAVERN mirror button pressed");
            }

            // GUILayout.Space(10);
            // GUILayout.Label("Mirror Position Across Axis");
            // EditorGUILayout.HelpBox("Object mirrors the source object's position reflected across a user determined axis.", MessageType.Info);
            // if (GUILayout.Button("Add Axis Mirror"))
            // {
            //     AddInteraction(typeof(MirrorFromAxis));
            //     Debug.Log("axis mirror button pressed");
            // }

            GUILayout.Space(10);
            GUILayout.Label("Evasive Motion");
            EditorGUILayout.HelpBox("Object moves away from an user determined target when it's within a distance (preserves height, y position value unchanged). Contains adjustable trigger distance, move away distance, and move away speed.", MessageType.Info);
            if (GUILayout.Button("Add Evasive Motion"))
            {
                AddInteraction(typeof(EvasiveMotion));
                // Debug.Log("retreat button pressed");
            }
        }

        private void AddInteraction(Type interaction)
        {
            foreach (GameObject obj in Selection.gameObjects)
            {
                // Debug.Log("Selected: " + obj.name);
                if (obj != null)
                {
                    obj.AddComponent(interaction);
                    // if(interaction == typeof(CavernMirrorMotion)) {
                    //     CavernMirrorMotion c = obj.GetComponent<CavernMirrorMotion>();
                    //     CavernRenderer cavernCam;
                    //     c.SetCamera(cavernCam);
                    // }
                    // Debug.Log("added: " + interaction);
                }
            }
        }
    }

}


