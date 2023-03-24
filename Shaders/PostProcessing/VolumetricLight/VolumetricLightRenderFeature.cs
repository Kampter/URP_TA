using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VolumetricLightRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class VLSettings
    {
        public RenderPassEvent passEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public ScriptableRenderPassInput requirements = ScriptableRenderPassInput.Color;
        [Header("VolumetricLightPass Settings")] [Space(10)]
        public Color colorTint = Color.red;
    }

    public VLSettings vlSettings= new VLSettings();
    private VolumetricLightPass volumetricLightPass;
    private bool requiresColor;
    private bool injectedBeforeTransparents;

    /// <inheritdoc/>
    public override void Create()
    {
        fullScreenPass = new FullScreenRenderPass();
        fullScreenPass.renderPassEvent = (RenderPassEvent)injectionPoint;

        // This copy of requirements is used as a parameter to configure input in order to avoid copy color pass
        

        requiresColor = (requirements & ScriptableRenderPassInput.Color) != 0;
        injectedBeforeTransparents = injectionPoint <= InjectionPoint.BeforeRenderingTransparents;

        if (requiresColor && !injectedBeforeTransparents)
        {
            // Removing Color flag in order to avoid unnecessary CopyColor pass
            // Does not apply to before rendering transparents, due to how depth and color are being handled until
            // that injection point.
            modifiedRequirements ^= ScriptableRenderPassInput.Color;
        }
        fullScreenPass.ConfigureInput(modifiedRequirements);
    }
    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        ScriptableRenderPassInput modifiedRequirements = vlSettings.requirements;
        volumetricLightPass.ConfigureInput(vlSettings.requirements);
        volumetricLightPass.Setup(renderer.cameraColorTargetHandle, vlSettings);
    }

    public override void Create()
    {
        volumetricLightPass = new VolumetricLightPass();
        volumetricLightPass.renderPassEvent = vlSettings.passEvent;
    }
    
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(volumetricLightPass);
    }
    
    protected override void Dispose(bool disposing)
    {
        volumetricLightPass.Dispose();
    }
    
    class VolumetricLightPass : ScriptableRenderPass
    {
        private ProfilingSampler profilingSampler = new ProfilingSampler(nameof(VolumetricLightPass));
        private static readonly string ShaderPath = "Kampter/VolumetricLight";
        private static readonly int BlitTextureID = Shader.PropertyToID("_BlitTexture");
        private static readonly int ColorTintID = Shader.PropertyToID("_ColorTint");
        private VLSettings vLSettings;
        private Material vLMaterial;
        private RTHandle source;
        private RTHandle copiedColor;
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            source = renderingData.cameraData.renderer.
            ConfigureTarget(source);
            var colorCopyDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            RenderingUtils.ReAllocateIfNeeded(ref copiedColor, colorCopyDescriptor, name: "_FullscreenPassColorCopy");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (vLMaterial == null)
                return;
            
            CommandBuffer cmd = CommandBufferPool.Get();
            var cameraData = renderingData.cameraData;
            
            using (new ProfilingScope(cmd, profilingSampler))
            {
                vLMaterial.SetColor(ColorTintID, vLSettings.colorTint);
                Blitter.BlitCameraTexture(cmd, source, copiedColor);
                vLMaterial.SetTexture(BlitTextureID, copiedColor);
                Blitter.BlitCameraTexture(cmd, copiedColor, source, vLMaterial, 0);
            }
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        public void Setup(RTHandle rendererCameraColorTargetHandle, VLSettings vLSettings)
        {
            this.vLSettings = vLSettings;
            Shader shader = Shader.Find(ShaderPath);
            vLMaterial = CoreUtils.CreateEngineMaterial(shader);
            source = rendererCameraColorTargetHandle;
        }

        public void Dispose()
        {
            source.Release();
            copiedColor.Release();
            CoreUtils.Destroy(vLMaterial);
        }
    }
}


