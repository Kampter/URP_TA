 Shader "Kampter/RayMarching"
{
    Properties
    {
        _BaseMap ("BaseMap", 2D) = "white" {}
        MAX_STEPS("MAX_STEPS", float) = 100//步进最大次数
        SURF_DIST("SURF_DIST", float) = 0.001//距离容差值
        MAX_DIST("MAX_DIST", float) = 100//步进的最远距离
    }
    SubShader
    {
        Tags {
            "RenderType"="Opaque" 
            "RenderPipeline"="UniversalRenderPipeline" 
        }

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct Attributes
            {
                half4 positionOS : POSITION;
                half2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                half4 positionCS : SV_POSITION;
                half3 positionWS : TEXCOORD0;
                half2 uv : TEXCOORD1;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseMap_ST;
                half MAX_STEPS;
                half SURF_DIST;
                half MAX_DIST;
            CBUFFER_END
            
            Varyings vert (Attributes IN)
            {
                const VertexPositionInputs vertex_position_inputs = GetVertexPositionInputs(IN.positionOS);
                
                Varyings OUT;
                OUT.positionCS = vertex_position_inputs.positionCS;
                OUT.positionWS = vertex_position_inputs.positionWS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half GetDisFromGeometry(half3 position)
            {
                return length(position) - 3;
            }

            half RayMarch(half3 rayOrigin, half3 rayDirection)
            {
                half disFromOrigin = 0;
                half disFromGeometry;
                for (int i = 0; i < MAX_STEPS; ++i)
                {
                    half3 position = rayOrigin + rayDirection * disFromOrigin;
                    disFromGeometry = GetDisFromGeometry(position);
                    disFromOrigin += disFromGeometry;
                    if (disFromGeometry < SURF_DIST || disFromOrigin > MAX_DIST)
                    {
                        break;
                    }
                }
                return disFromOrigin;
            }
            half3 GetNormal(half3 position)
            {
                return normalize(position);
            }

            half4 frag (Varyings IN) : SV_Target
            {
                half3 rayOrigin = _WorldSpaceCameraPos;
                half3 rayDirection = normalize(IN.positionWS - rayOrigin);
                half distance = RayMarch(rayOrigin, rayDirection);
                half4 color = 0;
                if (distance < MAX_DIST)
                {
                    half3 position = rayOrigin + rayDirection * distance;
                    color.rgb = GetNormal(position);
                }
                return color;
            }
            ENDHLSL
        }
    }
}
