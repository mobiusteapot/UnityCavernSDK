using UnityEngine;
using UnityEditor;
using System;

namespace Spelunx.Vive {
    public class ViveToolsPanel : EditorWindow {
        private UnityEngine.Object viveManager;
        private UnityEngine.Object viveTracker;
        private GameObject newViveManager;
        private int trackerCount;

        [MenuItem("CAVERN/VIVE Trackers")]
        public static void ShowWindow() {
            GetWindow<ViveToolsPanel>("VIVE Tracker Tools");
        }

        void OnGUI() {
            // === add Vive Tracker and Manager ===
            GUILayout.Label("VIVE Tracker", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Adds a new VIVE tracker to your scene and a new VIVE tracker manager to your scene if no tracker managers present.", MessageType.Info);

            TagAdder.AddTag("ViveManager");
            TagAdder.AddTag("ViveTracker");

            trackerCount = GameObject.FindGameObjectsWithTag("ViveTracker").Length;
            GUILayout.Label("Current trackers in scene: " + trackerCount);

            // === Add Vive Tracker to Scene ===
            if (GUILayout.Button("Add new Vive Tracker")) {
                // load from GUI input
                // objToSpawn = EditorGUILayout.ObjectField("Prefab", objToSpawn, typeof(Object), true);

                // load from path
                viveManager = (GameObject)AssetDatabase.LoadAssetAtPath("Packages/com.spelunx.cavern.vive-trackers/Prefabs/ViveTrackerManager.prefab", typeof(GameObject));
                viveTracker = (GameObject)AssetDatabase.LoadAssetAtPath("Packages/com.spelunx.cavern.vive-trackers/Prefabs/ViveTracker.prefab", typeof(GameObject));
                // Debug.Log("vive setup: " + viveManager.name + " " + viveTracker.name);
                // Debug.Log("Got " + objToSpawn.name + objToSpawn.GetType());

                // check if vive tracker manager is already present
                if (GameObject.FindGameObjectsWithTag("ViveManager").Length == 0) {
                    newViveManager = (GameObject)PrefabUtility.InstantiatePrefab(viveManager as GameObject);

                    // set vive manager to be in the CAVERN setup folder in the scene hierarchy
                    GameObject cavernSetup = GameObject.Find("CavernSetup");
                    if (cavernSetup != null) {
                        newViveManager.GetComponent<Transform>().parent = cavernSetup.transform;
                    }
                    // load in the debug keys
                    cavernSetup.AddComponent<ViveDebugKeys>();
                    // cavernSetup.GetComponent<DebugManager>().AddKeyManager(new ViveDebugKeys());
                }

                // instantiate a new vive tracker
                GameObject tracker = (GameObject)PrefabUtility.InstantiatePrefab(viveTracker as GameObject);
                tracker.GetComponent<ViveTracker>().SetOrigin(GameObject.FindGameObjectWithTag("ViveManager").transform);

            }

            //=== interaction building blocks ===
            GUILayout.Space(20);
            GUILayout.Label("VIVE Tracker Interaction Building Blocks", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("A collection of interactions using Vive Trackers.", MessageType.Info);

            GUILayout.Space(10);
            GUILayout.Label("Follow Interaction");
            EditorGUILayout.HelpBox("Object follows a target.", MessageType.Info);
            if (GUILayout.Button("Add Follow Interaction")) {
                AddInteraction<FollowInteraction>();
            }

            GUILayout.Space(10);
            GUILayout.Label("Orbit CAVERN Interaction");
            EditorGUILayout.HelpBox("Object orbits around the CAVERN, following an target.", MessageType.Info);
            if (GUILayout.Button("Add Orbit CAVERN Interaction")) {
                AddInteraction<OrbitCavernInteraction>();
            }

            GUILayout.Space(10);
            GUILayout.Label("Evade Interaction");
            EditorGUILayout.HelpBox("Object moves away from a target when it gets too close.", MessageType.Info);
            if (GUILayout.Button("Add Evade Interaction")) {
                AddInteraction<EvadeInteraction>();
            }

            GUILayout.Space(10);
            GUILayout.Label("Look At Interaction");
            EditorGUILayout.HelpBox("Object rotates to face the target.", MessageType.Info);
            if (GUILayout.Button("Add Look At Interaction")) {
                AddInteraction<LookAt>();
            }

            GUILayout.Space(10);
            GUILayout.Label("Zones");
            EditorGUILayout.HelpBox("Create distinct zones within the CAVERN, with a deadzone in the middle. This script gets attached to the ViveTrackerManager object, and zone information can be read from there.", MessageType.Info);
            if (GUILayout.Button("Add Zones")) {
                Zones component = GameObject.FindGameObjectWithTag("ViveManager").AddComponent(typeof(Zones)) as Zones;
                component.cavern = FindFirstObjectByType<CavernRenderer>();
            }
        }

        private void AddInteraction<T>() where T : Interaction {
            foreach (GameObject go in Selection.gameObjects) {
                if (go != null) {
                    T interaction = go.AddComponent<T>();

                    // Set the target to a vive tracker by default.
                    ViveTracker defaultViveTracker = FindFirstObjectByType<ViveTracker>();
                    if (defaultViveTracker != null) {
                        interaction.SetTarget(defaultViveTracker.transform);
                    }

                    // CavernInteraction specific stuff.
                    if (typeof(T).IsSubclassOf(typeof(CavernInteraction))) {
                        CavernInteraction cavernInteraction = interaction as CavernInteraction;
                        cavernInteraction.SetCavernRenderer(FindFirstObjectByType<CavernRenderer>());
                    }
                }
            }
        }
    }
}