using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Spelunx
{
    public class ScreenSpaceCanvasRenderer : MonoBehaviour
    {

        [SerializeField]
        private Texture uiDoublerTexture;
        [SerializeField]
        private Shader uiDoublerShader;
        private Material uiDoublerMaterial;
        [SerializeField]
        private float uiDoublerEyeOffset = 0f;
        [SerializeField]
        private Camera renderCamera;
        private DirectCanvasRenderPass directCanvasRenderPass;

        private void Awake()
        {
            // Initialise material.
            uiDoublerMaterial = new Material(uiDoublerShader);
            uiDoublerMaterial.SetTexture("_MainTex", uiDoublerTexture);
            uiDoublerMaterial.SetFloat("_3d_offset", uiDoublerEyeOffset);
            directCanvasRenderPass = new DirectCanvasRenderPass(uiDoublerMaterial);
        }

        private void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Update()
        {
            uiDoublerMaterial.SetFloat("_3d_offset", uiDoublerEyeOffset);
        }

        private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (camera == renderCamera)
            {
                camera.GetUniversalAdditionalCameraData().scriptableRenderer.EnqueuePass(directCanvasRenderPass);
            }
        }

        private void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
        {
        }
    }
}
