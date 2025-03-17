using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;
using static Spelunx.CavernRenderer;

namespace Spelunx
{

    public class CavernRenderPass : ScriptableRenderPass
    {



        private const string PassName = "CavernRenderPass";
        private Material blitMaterial;
        // Todo: Move some of the relevant setup for specifically texture/material setup from CavernRenderer to here

        private bool hasScreenViewer = false;
        private RenderTexture screenViewerTexture;
        private RTHandle screenViewerRTHandle;
        private Material screenViewerMaterial;

        public CavernRenderPass()
        {
            requiresIntermediateTexture = true;
        }

        public void Setup(Material material, RenderTexture[] cubemaps)
        {
            if(cubemaps == null)
            {
                Debug.LogError("Cavern cubemaps are null");
                return;
            }
            if(material == null)
            {
                Debug.LogError("Blit material is null");
                return;
            }
            blitMaterial = material;
            blitMaterial.SetTexture("_CubemapNorth", cubemaps[(int)CubemapIndex.North]);
            blitMaterial.SetTexture("_CubemapSouth", cubemaps[(int)CubemapIndex.South]);
            blitMaterial.SetTexture("_CubemapEast", cubemaps[(int)CubemapIndex.East]);
            blitMaterial.SetTexture("_CubemapWest", cubemaps[(int)CubemapIndex.West]);
        }

        public void SetupScreenViewer(Material material, RenderTexture screenViewerTexture)
        {
            if(screenViewerTexture == null)
            {
                Debug.LogError("Screen viewer texture is null");
                return;
            }
            if(material == null)
            {
                Debug.LogError("Screen viewer material is null");
                return;
            }
            
            this.screenViewerMaterial = material;
            this.screenViewerTexture = screenViewerTexture;
            this.screenViewerMaterial.SetTexture("_MainTex", screenViewerTexture);
            hasScreenViewer = true;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {

            var resourceData = frameData.Get<UniversalResourceData>();

            if(resourceData.isActiveTargetBackBuffer)
            {
                return;
            }

            var source = resourceData.activeColorTexture;

            var destinationDesc = renderGraph.GetTextureDesc(source);
            destinationDesc.name = PassName;
            destinationDesc.clearBuffer = false;

            TextureHandle destination = renderGraph.CreateTexture(destinationDesc);

            RenderGraphUtils.BlitMaterialParameters para = new(source, destination, blitMaterial, 0);
            renderGraph.AddBlitPass(para, passName: PassName);


#if UNITY_EDITOR
            if(hasScreenViewer)
            {
                RenderTextureDescriptor screenViewerProperties = new RenderTextureDescriptor(screenViewerTexture.width, screenViewerTexture.height, screenViewerTexture.format, 0);
                RenderingUtils.ReAllocateHandleIfNeeded(ref screenViewerRTHandle, screenViewerProperties, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "ScreenViewer");

                TextureHandle screenViewerHandle = renderGraph.ImportTexture(screenViewerRTHandle);

                RenderGraphUtils.BlitMaterialParameters screenViewerPara = new(source, screenViewerHandle, screenViewerMaterial, 1);
                renderGraph.AddBlitPass(screenViewerPara, passName: "ScreenViewer");
            }
#endif

            resourceData.cameraColor = destination;
        }

        public void Dispose()
        {
#if UNITY_EDITOR
            if(hasScreenViewer)
            {
                screenViewerRTHandle.Release();
            }
#endif
        }
    }
}
