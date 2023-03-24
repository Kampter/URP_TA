Shader "Kampter/GaussianBlur"
{
    Properties
    {
         _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader
    {
        ZTest Always
        Cull Off
        ZWrite Off
        
        Tags
        {
           "RenderPipeline" = "UniversalRenderPipeline"
        }
        LOD 100

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        
        struct Attributes
        {
            float4 positionOS : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct Varyings
        {
            float2 uv[5] : TEXCOORD0;
            float4 positionCS : SV_POSITION;
        };

        TEXTURE2D(_MainTex);             SAMPLER(sampler_MainTex);

        CBUFFER_START(UnityPerMaterial)
            float _BlurSize;
            float4 _MainTex_TexelSize;
        CBUFFER_END
        
        Varyings VertBlurVertical(Attributes v)
        {
            Varyings o;
            o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
            float2 uv = v.uv;

            o.uv[0] = uv;
            o.uv[1] = uv + float2(0.0, _MainTex_TexelSize.y * 1.0) * _BlurSize;
            o.uv[2] = uv - float2(0.0, _MainTex_TexelSize.y * 1.0) * _BlurSize;
            o.uv[3] = uv + float2(0.0, _MainTex_TexelSize.y * 2.0) * _BlurSize;
            o.uv[4] = uv - float2(0.0, _MainTex_TexelSize.y * 2.0) * _BlurSize;

            return o;
        }

        Varyings VertBlurHorizontal(Attributes v)
        {
            Varyings o = (Varyings)0;
            o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
            float2 uv = v.uv;

            o.uv[0] = uv;
            o.uv[1] = uv + float2(_MainTex_TexelSize.x * 1.0, 0.0) * _BlurSize;
            o.uv[2] = uv - float2(_MainTex_TexelSize.x * 1.0, 0.0) * _BlurSize;
            o.uv[3] = uv + float2(_MainTex_TexelSize.x * 2.0, 0.0) * _BlurSize;
            o.uv[4] = uv - float2(_MainTex_TexelSize.x * 2.0, 0.0) * _BlurSize;
            return o;
        }

        float4 fragBlur(Varyings i) : SV_Target
        {
            float weight[3] = {0.4026, 0.2442, 0.0545};

            //中心像素值
            float3 sum = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv[0]).rgb * weight[0];
            
            for (int it = 1; it < 3; it++)
            {
                sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv[it * 2 - 1]).rgb * weight[it];
                sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv[it * 2]).rgb * weight[it];
            }

            return float4(sum, 1.0);
        }
        ENDHLSL

        Pass
        {
            Name "GaussianPass0"
            HLSLPROGRAM            
            #pragma vertex VertBlurVertical
            #pragma fragment fragBlur
            ENDHLSL
        }

        Pass
        {
            Name "GaussianPass1"
            HLSLPROGRAM            
            #pragma vertex VertBlurHorizontal
            #pragma fragment fragBlur
            ENDHLSL
        }
    }
    Fallback Off
}