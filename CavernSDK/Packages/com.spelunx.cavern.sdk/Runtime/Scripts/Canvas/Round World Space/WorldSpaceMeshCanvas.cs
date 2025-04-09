using UnityEngine;

namespace Spelunx
{
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class WorldSpaceMeshCanvas : MonoBehaviour
    {
        [SerializeField, Tooltip("A reference to the Cavern Camera")]
        private CavernRenderer cavernRenderer;

        [SerializeField, Tooltip("Distance from the screen to render. 0 is purely at the center, 1 is at the boundry"), Min(0)]
        private float distance = 1.0f;


        [Header("Don't touch these")]
        [SerializeField]
        private Material baseMat;

        [SerializeField]
        private Mesh mesh;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            GetComponent<MeshRenderer>().material = baseMat;
            mesh = cavernRenderer.GenerateMesh();
            GetComponent<MeshFilter>().mesh = mesh;
            transform.localScale = new(distance, distance, distance);
        }
    }
}
