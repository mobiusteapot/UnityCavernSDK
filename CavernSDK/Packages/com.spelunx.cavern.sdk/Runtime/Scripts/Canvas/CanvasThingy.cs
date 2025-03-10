using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Spelunx
{
    public class CanvasThingy : MonoBehaviour
    {


        [SerializeField] private Shader shader;
        private Material material;
        private RenderTexture tex;

        private void Awake()
        {
            // Initialise material.
            material = new Material(shader);
            material.SetTexture("_MainTex", GetComponent<Camera>().targetTexture);
        }

        private void OnEnable()
        {
            RenderPipelineManager.beginContextRendering += OnBeginContextRendering;
            RenderPipelineManager.endContextRendering += OnEndContextRendering;
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginContextRendering -= OnBeginContextRendering;
            RenderPipelineManager.endContextRendering -= OnEndContextRendering;
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        private void OnBeginContextRendering(ScriptableRenderContext context, List<Camera> cameras) { }

        private void OnEndContextRendering(ScriptableRenderContext context, List<Camera> cameras)
        {
            // Graphics.Blit(null, material);
        }

        private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera) { }

        private void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (camera == GetComponent<Camera>())
            {
                // RenderTexture t = camera.targetTexture;
                // Graphics.DrawTexture(new Rect(0, 0, 5760, 1080), t, new Rect(0, 0, 5760, 1080), 0, 5760, 0, 2160);
                // Graphics.DrawTexture(new Rect(0, 0, 5760, 1080), t, new Rect(0, 1080, 5760, 2160), 0, 5760, 0, 2160);
                Graphics.Blit(null, material);
            }
        }
    }
}
