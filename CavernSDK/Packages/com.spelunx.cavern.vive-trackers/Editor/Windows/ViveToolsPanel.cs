using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace Spelunx.Vive
{
    public class ViveToolsPanel : EditorWindow
    {
        private UnityEngine.Object viveManagerPrefab;
        private UnityEngine.Object viveTrackerPrefab;
        private GameObject viveManagerInstance;
        private int trackerCount;

        private const string VIVE_MANAGER_TAG = "ViveManager";
        private const string VIVE_TRACKER_TAG = "ViveTracker";

        [MenuItem("CAVERN/VIVE Trackers Tools", false, 101)]
        public static void ShowWindow()
        {
            GetWindow<ViveToolsPanel>("VIVE Tracker Tools");
        }

        private void OnGUI()
        {
            TagUtil.AddTag(VIVE_MANAGER_TAG);
            TagUtil.AddTag(VIVE_TRACKER_TAG);

            // === add Vive Tracker and Manager ===
            GUILayout.Label("VIVE Tracker", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Adds a new VIVE tracker to your scene and a new VIVE tracker manager to your scene if no tracker managers present.", MessageType.Info);

            trackerCount = GameObject.FindGameObjectsWithTag(VIVE_TRACKER_TAG).Length;
            GUILayout.Label("Current trackers in scene: " + trackerCount);

            // === Add Vive Tracker to Scene ===
            if (GUILayout.Button("Add new Vive Tracker"))
            {
                // load from path
                viveManagerPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Packages/com.spelunx.cavern.vive-trackers/Prefabs/ViveTrackerManager.prefab", typeof(GameObject));
                viveTrackerPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Packages/com.spelunx.cavern.vive-trackers/Prefabs/ViveTracker.prefab", typeof(GameObject));

                // check if vive tracker manager is already present
                if (GameObject.FindGameObjectsWithTag(VIVE_MANAGER_TAG).Length == 0)
                {
                    viveManagerInstance = (GameObject)PrefabUtility.InstantiatePrefab(viveManagerPrefab as GameObject);

                    // set vive manager to be in the CAVERN setup folder in the scene hierarchy
                    GameObject cavernSetup = GameObject.Find("CavernSetup");
                    if (cavernSetup != null)
                    {
                        viveManagerInstance.GetComponent<Transform>().parent = cavernSetup.transform;
                    }
                    // load in the debug keys
                    cavernSetup.AddComponent<ViveDebugKeys>();
                }

                // instantiate a new vive tracker
                GameObject viveTrackerInstance = (GameObject)PrefabUtility.InstantiatePrefab(viveTrackerPrefab as GameObject);
                viveTrackerInstance.GetComponent<ViveTracker>().SetOrigin(GameObject.FindGameObjectWithTag(VIVE_MANAGER_TAG).transform);
            }

            //=== interaction building blocks ===
            GUILayout.Space(20);
            GUILayout.Label("VIVE Tracker Interaction Building Blocks", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("A collection of interactions using Vive Trackers.", MessageType.Info);

            GUILayout.Space(10);
            GUILayout.Label("Follow Interaction");
            EditorGUILayout.HelpBox("Object follows a target.", MessageType.Info);
            if (GUILayout.Button("Add Follow Interaction"))
            {
                AddInteraction<FollowInteraction>();
            }

            GUILayout.Space(10);
            GUILayout.Label("Orbit CAVERN Interaction");
            EditorGUILayout.HelpBox("Object orbits around the CAVERN, following an target.", MessageType.Info);
            if (GUILayout.Button("Add Orbit CAVERN Interaction"))
            {
                AddInteraction<OrbitCavernInteraction>();
            }

            GUILayout.Space(10);
            GUILayout.Label("Evade Interaction");
            EditorGUILayout.HelpBox("Object moves away from a target when it gets too close.", MessageType.Info);
            if (GUILayout.Button("Add Evade Interaction"))
            {
                AddInteraction<EvadeInteraction>();
            }

            GUILayout.Space(10);
            GUILayout.Label("Look At Interaction");
            EditorGUILayout.HelpBox("Object rotates to face the target.", MessageType.Info);
            if (GUILayout.Button("Add Look At Interaction"))
            {
                AddInteraction<LookAt>();
            }

            GUILayout.Space(10);
            GUILayout.Label("Zones");
            EditorGUILayout.HelpBox("Create distinct zones within the CAVERN, with a deadzone in the middle. This script gets attached to the ViveTrackerManager object, and zone information can be read from there.", MessageType.Info);
            if (GUILayout.Button("Add Zones"))
            {
                Zones component = GameObject.FindGameObjectWithTag(VIVE_MANAGER_TAG).AddComponent(typeof(Zones)) as Zones;
                // Add CAVERN renderer
                component.cavern = FindFirstObjectByType<CavernRenderer>();
                // Add existing trackers in the scene by default
                ViveTracker[] trackers = FindObjectsByType<ViveTracker>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                if (trackers != null)
                {
                    component.zonedTrackers = new Zones.ZonedTracker[trackers.Length];
                    for (int i = 0; i < trackers.Length; i++)
                    {
                        component.zonedTrackers[i].transform = trackers[i].transform;
                    }
                }
            }
        }

        private void AddInteraction<T>() where T : Interaction
        {
            foreach (GameObject go in Selection.gameObjects)
            {
                if (go != null)
                {
                    T interaction = go.AddComponent<T>();

                    // Set the target to a vive tracker by default.
                    ViveTracker defaultViveTracker = FindFirstObjectByType<ViveTracker>();
                    if (defaultViveTracker != null)
                    {
                        interaction.SetTarget(defaultViveTracker.transform);
                    }

                    // CavernInteraction specific stuff.
                    if (typeof(T).IsSubclassOf(typeof(CavernInteraction)))
                    {
                        CavernInteraction cavernInteraction = interaction as CavernInteraction;
                        cavernInteraction.SetCavernRenderer(FindFirstObjectByType<CavernRenderer>());
                    }
                }
            }
        }
    }
}