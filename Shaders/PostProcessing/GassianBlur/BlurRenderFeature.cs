using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BlurRenderFeature : ScriptableRendererFeature
{    
    public class BlurSettings
    {
        public Shader blurShader;
        public RenderPassEvent passEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        [Header("Blur Settings")] [Space(10)]
        [Range(1, 8)] public int blurDownSample = 1;
        [Range(1,16)] public int iteration = 8;
        [Range(0,4)] public float blurSize = 1.5f;
    }
    
    class CustomRenderPass : ScriptableRenderPass
    {
        private static readonly string renderTag = "GaussianBlur";
        private static readonly int _BlurSize = Shader.PropertyToID("_BlurSize");
        private BlurSettings _blurSettings;
        private Material _blurMaterial;
        private RTHandle source;
        private RTHandle blurTex1;
        private RTHandle blurTex2;

        public CustomRenderPass(BlurSettings blurSettings)
        {
            _blurSettings = blurSettings;
        }
        
        public void setSource(RTHandle cameraColorTargetHandle)
        {
            source = cameraColorTargetHandle;
            _blurMaterial = CoreUtils.CreateEngineMaterial(_blurSettings.blurShader);
        }
        
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(renderTag);
            RenderTextureDescriptor cameraTextureDesc = renderingData.cameraData.cameraTargetDescriptor;
            cameraTextureDesc.depthBufferBits = 0;
            cameraTextureDesc.width /= _blurSettings.blurDownSample;
            cameraTextureDesc.height /= _blurSettings.blurDownSample;
            RenderingUtils.ReAllocateIfNeeded(ref blurTex1, cameraTextureDesc, name: "blurTex1");
            RenderingUtils.ReAllocateIfNeeded(ref blurTex2, cameraTextureDesc, name: "blurTex2");
            
            _blurMaterial.SetFloat(_BlurSize, _blurSettings.blurSize);
            Blitter.BlitCameraTexture(cmd, source, blurTex1);
            for (int i = 0; i < _blurSettings.iteration; i++)
            {
                Blitter.BlitCameraTexture(cmd, blurTex1, blurTex2, _blurMaterial, 0);
                Blitter.BlitCameraTexture(cmd, blurTex2, blurTex1, _blurMaterial, 1);
            }
            Blitter.BlitCameraTexture(cmd, blurTex1, source);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }
    }

    CustomRenderPass m_ScriptablePass;
    public BlurSettings blurSettings = new BlurSettings();
    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        m_ScriptablePass.renderPassEvent = blurSettings.passEvent;
        m_ScriptablePass.setSource(renderer.cameraColorTargetHandle);
    }
    
    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass(blurSettings);
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


