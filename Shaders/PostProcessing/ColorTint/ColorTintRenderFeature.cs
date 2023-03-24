using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ColorTintRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class ColorTintSettings
    {
        public Shader colorTintShader;
        public RenderPassEvent passEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public ScriptableRenderPassInput requirements = ScriptableRenderPassInput.Color;
        [Header("ColorTint Settings")] [Space(10)]
        public Color colorTint = Color.red;
    }
    
    public ColorTintSettings colorTintSettings = new ColorTintSettings();
    private ColorTintPass colorTintPass;

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        colorTintPass.ConfigureInput(colorTintSettings.requirements);
        colorTintPass.Setup(renderer.cameraColorTargetHandle, colorTintSettings);
    }
    public override void Create()
    {
        colorTintPass = new ColorTintPass();
        colorTintPass.renderPassEvent = colorTintSettings.passEvent;
    }
    
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(colorTintPass);
    }
    protected override void Dispose(bool disposing)
    {
        colorTintPass.Dispose();
    }
    
    public class ColorTintPass : ScriptableRenderPass
    {
        private ProfilingSampler profilingSampler = new ProfilingSampler(nameof(ColorTintPass));
        private ColorTintSettings colorTintSettings;
        private Material colorTintMaterial;
        private static readonly int ColorTintID = Shader.PropertyToID("_ColorTint");
        private static readonly int BlitTextureID = Shader.PropertyToID("_BlitTexture");
        private RTHandle source;
        private RTHandle copiedColor;

        public void Setup(RTHandle colorHandle, ColorTintSettings colorTintSettings)
        {
            this.colorTintSettings = colorTintSettings;
            Shader shader = colorTintSettings.colorTintShader;
            if (shader == null) 
                return;
            colorTintMaterial = CoreUtils.CreateEngineMaterial(shader);
            source = colorHandle;
        }

        public void Dispose()
        {
            source.Release();
            copiedColor.Release();
            CoreUtils.Destroy(colorTintMaterial);
        }
        
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ConfigureTarget(source);
            var colorCopyDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            colorCopyDescriptor.depthBufferBits = (int) DepthBits.None;
            RenderingUtils.ReAllocateIfNeeded(ref copiedColor, colorCopyDescriptor, name: "_FullscreenPassColorCopy");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (colorTintMaterial == null)
                return;
            
            CommandBuffer cmd = CommandBufferPool.Get();
            var cameraData = renderingData.cameraData;
            
            using (new ProfilingScope(cmd, profilingSampler))
            {
                colorTintMaterial.SetColor(ColorTintID, colorTintSettings.colorTint);
                Blitter.BlitCameraTexture(cmd, source, copiedColor);
                colorTintMaterial.SetTexture(BlitTextureID, copiedColor);
                Blitter.BlitCameraTexture(cmd, copiedColor, source, colorTintMaterial, 0);
            }
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }
}

