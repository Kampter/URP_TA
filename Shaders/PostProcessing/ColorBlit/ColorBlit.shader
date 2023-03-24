Shader "ColorBlit"
{
        SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZWrite Off Cull Off
        Pass
        {
            Name "ColorBlitPass"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/BlitColorAndDepth.hlsl"

            #pragma vertex Vert
            #pragma fragment frag
            float _Intensity;
            
            half4 frag (Varyings input) : SV_Target
            {
                half4 color = FragColorAndDepth(input).color  * float4(0, _Intensity, 0, 1);
                return color;
            }

            ENDHLSL
        }
    }
}