Shader "Kampter/VolumetricLight"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZWrite Off Cull Off
        Pass
        {
            Name "ColorTintPass"
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl" 
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/BlitColorAndDepth.hlsl"
            #pragma vertex Vert
            #pragma fragment frag

            float4 _ColorTint;

            half4 frag (Varyings input) : SV_Target
            {
                half4 color = FragColorAndDepth(input).color;
                return color;
            }
            ENDHLSL
        }
    }
}