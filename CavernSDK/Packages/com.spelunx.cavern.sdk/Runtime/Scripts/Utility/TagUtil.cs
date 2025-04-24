using UnityEditor;
using UnityEngine;

namespace Spelunx {
    public class TagUtil {
        private TagUtil() { }

        public static void AddTag(string tag) {
            UnityEngine.Object[] asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if ((asset != null) && (asset.Length > 0)) {
                SerializedObject so = new SerializedObject(asset[0]);
                SerializedProperty tags = so.FindProperty("tags");

                // check if tag is already present in tag list
                for (int i = 0; i < tags.arraySize; ++i) {
                    if (tags.GetArrayElementAtIndex(i).stringValue == tag) {
                        return;
                    }
                }

                tags.InsertArrayElementAtIndex(tags.arraySize);
                tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
                so.ApplyModifiedProperties();
                so.Update();
            }
        }
    }
}