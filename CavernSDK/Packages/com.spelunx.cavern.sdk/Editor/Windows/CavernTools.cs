using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Spelunx
{
    // [MenuItem("CAVERN/Tools")]
    // [CustomEditor(typeof(CavernRenderer))]
    public class CavernTools : Editor
    {
        public VisualTreeAsset VisualTree;
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();

            // add everything in UI builder to root element
            VisualTree.CloneTree(root);

            return root;
            // return base.CreateInspectorGUI();
        }
    }
}
