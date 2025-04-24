using UnityEngine;
using UnityEditor;

namespace Spelunx.Orbbec {
    public class OrbbecToolsPanel : EditorWindow {
        [MenuItem("CAVERN/ORBBEC Tools", false, 102)]
        public static void ShowWindow() {
            GetWindow<OrbbecToolsPanel>("ORBBEC Tools");
        }

        private void OnGUI() {
            GUILayout.Label("ORBBEC Sensors", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("You should only have 1 BodyTracker & BodyTrackerManager in the scene!", MessageType.Warning);

            if (GUILayout.Button("Add BodyTracker & BodyTrackerManager")) { 
                GameObject bodyTrackerPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Packages/com.spelunx.cavern.orbbec.sdk/Prefabs/BodyTracker.prefab", typeof(GameObject));
                GameObject bodyTrackerManagerPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Packages/com.spelunx.cavern.orbbec.sdk/Prefabs/BodyTrackerManager.prefab", typeof(GameObject));
                GameObject bodyTrackerInstance = (GameObject)PrefabUtility.InstantiatePrefab(bodyTrackerPrefab);
                GameObject bodyTrackerManagerInstance = (GameObject)PrefabUtility.InstantiatePrefab(bodyTrackerManagerPrefab);
                bodyTrackerManagerInstance.GetComponent<BodyTrackerManager>().SetBodyTracker(bodyTrackerInstance.GetComponent<BodyTracker>());
            }

            BodyTracker bodyTracker = FindFirstObjectByType<BodyTracker>();
            if (bodyTracker == null) { return; }

            if (GUILayout.Button("Add (Example) BodyTrackerAvatar")) {
                GameObject bodyTrackerAvatarPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Packages/com.spelunx.cavern.orbbec.sdk/Prefabs/BodyTrackerAvatar.prefab", typeof(GameObject));
                GameObject bodyTrackerAvatarInstance = (GameObject)PrefabUtility.InstantiatePrefab(bodyTrackerAvatarPrefab);
                bodyTrackerAvatarInstance.GetComponent<BodyTrackerAvatar>().SetBodyTracker(bodyTracker);
                bodyTrackerAvatarInstance.GetComponent<BodyTrackerAvatar>().SetSkeletonRootJoint(bodyTracker.GetRootJoint());
            }
        }
    }
}