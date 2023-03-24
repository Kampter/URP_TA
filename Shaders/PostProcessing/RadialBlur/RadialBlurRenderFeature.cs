using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RadialBlurRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public Shader shader;
    }
    
    public Settings settings = new Settings();
    RadialBlurPass radialBlurPass;           // 定义我们创建出Pass
    
    public override void Create()
    {
        radialBlurPass = new RadialBlurPass(RenderPassEvent.BeforeRenderingPostProcessing, settings.shader);    // 初始化 我们的渲染层级和Shader
    }
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(radialBlurPass);
    }
}


public class RadialBlurPass : ScriptableRenderPass
{
    static readonly string RenderTag = "Radial Blur Effects";                         // 设置渲染标签
    RadialBlurVolume radialBlurVolume;                                                  // 定义组件类型
    Material Radialmaterial;                                                      // 后处理材质// 设置当前渲染目标
    RenderTargetIdentifier BlurTex;
    RenderTargetIdentifier Temp;

    public RadialBlurPass(RenderPassEvent evt, Shader blurshader)
    {
        renderPassEvent = evt;
        var shader = blurshader;

        if (shader == null)
        {
            Debug.LogError("没有指定Shader");
            return;
        }
        Radialmaterial = CoreUtils.CreateEngineMaterial(blurshader);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (Radialmaterial == null)
        {
            Debug.LogError("材质初始化失败");
            return;
        }

        if (!renderingData.cameraData.postProcessEnabled)
        {
            Debug.LogError("");
            return;
        }

        var stack = VolumeManager.instance.stack;                                    // 传入 volume
        radialBlurVolume = stack.GetComponent<RadialBlurVolume>();                     // 获取到后处理组件

        if (radialBlurVolume == null)
        {
            Debug.LogError("获取组件失败");
            return;
        }

        var cmd = CommandBufferPool.Get(RenderTag);    // 渲染标签

        Render(cmd, ref renderingData);                 // 调用渲染函数

        context.ExecuteCommandBuffer(cmd);              // 执行函数，回收。
        CommandBufferPool.Release(cmd);

    }

    void Render(CommandBuffer cmd, ref RenderingData renderingData)
    {
        RenderTargetIdentifier sourceRT = renderingData.cameraData.renderer.cameraColorTarget;                 // 定义RT
        RenderTextureDescriptor inRTDesc = renderingData.cameraData.cameraTargetDescriptor;
        inRTDesc.depthBufferBits = 0;                                                                          // 清除深度


        // 定义屏幕尺寸
        var width = (int)(inRTDesc.width / radialBlurVolume.RTDownSampling.value);
        var height = (int)(inRTDesc.height / radialBlurVolume.RTDownSampling.value);

        Radialmaterial.SetFloat("_Loop", radialBlurVolume.BlurTimes.value);             // Shader变量  和 Volume 组件属性 绑定
        Radialmaterial.SetFloat("_X", radialBlurVolume.X.value);                         // Shader变量  和 Volume 组件属性 绑定
        Radialmaterial.SetFloat("_Y", radialBlurVolume.Y.value);                         // Shader变量  和 Volume 组件属性 绑定
        Radialmaterial.SetFloat("_Blur", radialBlurVolume.BlurRange.value);             // Shader变量  和 Volume 组件属性 绑定
        Radialmaterial.SetFloat("_BufferRadius", radialBlurVolume.BufferRadius.value);             // Shader变量  和 Volume 组件属性 绑定

        int TempID = Shader.PropertyToID("Temp");
        int BlurTexID = Shader.PropertyToID("_BlurTex");              // 临时
        // 获取一张临时RT
        cmd.GetTemporaryRT(TempID, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.DefaultHDR); //申请一个临时图像，并设置相机rt的参数进去
        cmd.GetTemporaryRT(BlurTexID, inRTDesc);   // 模糊图

        BlurTex = new RenderTargetIdentifier(BlurTexID);
        Temp = new RenderTargetIdentifier(TempID);

        cmd.Blit(sourceRT, Temp);                              // 摄像机渲染的图储存到 Temp1
        cmd.Blit(Temp, BlurTex, Radialmaterial, 0);            // 临时图像 进行径向模糊
        cmd.Blit(BlurTex, sourceRT);            // 模糊 和原图混合
    }
} 