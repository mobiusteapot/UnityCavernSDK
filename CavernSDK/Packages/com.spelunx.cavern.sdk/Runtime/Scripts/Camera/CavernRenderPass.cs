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
        private Material blitMaterial;

        public CavernRenderPass(Material blitMaterial)
        {
            this.blitMaterial = blitMaterial;
            this.requiresIntermediateTexture = true;
            this.renderPassEvent = RenderPassEvent.AfterRendering;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            const string name = "CavernRenderPass";

            // Get source.
            var resourceData = frameData.Get<UniversalResourceData>();
            if (resourceData.isActiveTargetBackBuffer) { return; }
            var source = resourceData.activeColorTexture;

            // Get destination.
            var destinationDesc = renderGraph.GetTextureDesc(source);
            destinationDesc.name = name;
            destinationDesc.clearBuffer = false;
            TextureHandle destination = renderGraph.CreateTexture(destinationDesc);

            // Add blit pass.
            RenderGraphUtils.BlitMaterialParameters para = new(source, destination, blitMaterial, 0);
            renderGraph.AddBlitPass(para, passName: name);

            resourceData.cameraColor = destination;
        }
    }
}