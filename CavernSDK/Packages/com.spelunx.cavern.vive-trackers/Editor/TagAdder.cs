using UnityEngine;
using UnityEditor;

namespace Spelunx.Vive
{
    public class TagAdder : MonoBehaviour
    {
        public static void AddTag(string tag)
        {
            UnityEngine.Object[] asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if ((asset != null) && (asset.Length > 0))
            {
                SerializedObject so = new SerializedObject(asset[0]);
                SerializedProperty tags = so.FindProperty("tags");

                // check if tag is already present in tag list
                for (int i = 0; i < tags.arraySize; ++i)
                {
                    if (tags.GetArrayElementAtIndex(i).stringValue == tag)
                    {
                        return;
                    }
                }

                tags.InsertArrayElementAtIndex(0);
                tags.GetArrayElementAtIndex(0).stringValue = tag;
                so.ApplyModifiedProperties();
                so.Update();
            }
        }
    }
}