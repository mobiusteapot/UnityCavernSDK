using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace Spelunx
{
    // Todo: Execute to game view in edit mode?
    public class CavernRenderPass : ScriptableRenderPass
    {



        private const string PassName = "CavernRenderPass";
        private Material blitMaterial;
        // Todo: Move some of the relevant setup for specifically texture/material setup from CavernRenderer to here
        private RenderTexture[] cubemaps;
        private RenderTexture screenViewerTexture;

        public CavernRenderPass()
        {
            requiresIntermediateTexture = true;
        }

        public void Setup(Material material, RenderTexture[] cubemaps)
        {
            blitMaterial = material;
            this.cubemaps = cubemaps;
        }
        // Currently unimplemented
        public void SetupScreenViewer(Material material, RenderTexture screenViewerTexture)
        {
            blitMaterial = material;
            this.screenViewerTexture = screenViewerTexture;
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

            resourceData.cameraColor = destination;
        }
    }
}
