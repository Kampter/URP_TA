using Palmmedia.ReportGenerator.Core;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GassianBlurRenderPassFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class BlurSettings
    {
        public Shader blurShader;
        public RenderPassEvent passEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        [Header("Blur Settings")] [Space(10)]
        [Range(1, 8)] public int blurDownSample = 1;
        [Range(1,16)] public int iteration = 8;
        [Range(0,4)] public float blurSize = 1.5f;
    }
    class GassianBlurPass : ScriptableRenderPass
    {
        private static readonly string renderTag = "GaussianBlur";
        private static readonly int _BlurSize = Shader.PropertyToID("_BlurSize");
        private BlurSettings _blurSettings;
        private Material _blurMaterial;
        private RenderTargetIdentifier source;
        private RenderTargetHandle blurTex1;
        private RenderTargetHandle blurTex2;
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(renderTag);
            RenderTextureDescriptor cameraTextureDesc = renderingData.cameraData.cameraTargetDescriptor;
            cameraTextureDesc.depthBufferBits = 0;
            var width = (int)cameraTextureDesc.width / _blurSettings.blurDownSample;
            var height = (int)cameraTextureDesc.height / _blurSettings.blurDownSample;
            
            cmd.GetTemporaryRT(blurTex1.id, width, height, 0, FilterMode.Bilinear);
            cmd.GetTemporaryRT(blurTex2.id, width, height, 0, FilterMode.Bilinear);
            
            cmd.SetGlobalFloat(_BlurSize, _blurSettings.blurSize); 
            
            cmd.Blit(source, blurTex1.id);
            for (int i = 0; i < _blurSettings.iteration; i++)
            {
                cmd.Blit(blurTex1.id, blurTex2.id, _blurMaterial, 0);
                cmd.Blit(blurTex2.id, blurTex1.id, _blurMaterial, 1);
            }
            cmd.Blit(blurTex1.id, source);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(blurTex1.id);
            cmd.ReleaseTemporaryRT(blurTex2.id);
        }

        public GassianBlurPass(BlurSettings blurSettings)
        {
            this._blurSettings = blurSettings;
            blurTex1.Init("blurTex1");
            blurTex2.Init("blurTex2");
        }
        public void setSource(RenderTargetIdentifier rendererCameraColorTarget)
        {
            this.source = rendererCameraColorTarget;
            this._blurMaterial = CoreUtils.CreateEngineMaterial(_blurSettings.blurShader);
        }
    }

    public BlurSettings blurSettings = new BlurSettings();
    GassianBlurPass gassianBlurPass;

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        gassianBlurPass.renderPassEvent = blurSettings.passEvent;
        gassianBlurPass.setSource(renderer.cameraColorTarget);
    }

    public override void Create()
    {
        gassianBlurPass = new GassianBlurPass(blurSettings);
    }
    
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(gassianBlurPass);
    }
}


