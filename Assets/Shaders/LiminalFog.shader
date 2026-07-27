Shader "Echoes/LiminalFog" {
    Properties {
        _Color ("Base Color", Color) = (0.11, 0.14, 0.19, 1.0)
        _FogColor ("Fog Color", Color) = (0.11, 0.14, 0.19, 1.0)
        _FogDensity ("Fog Density", Float) = 0.02
        _FogStart ("Fog Start Distance", Float) = 5.0
        _FogEnd ("Fog End Distance", Float) = 20.0
    }
    SubShader {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 200

        Pass {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes {
                float4 position : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct Varyings {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float3 worldPos : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _FogColor;
                float _FogDensity;
                float _FogStart;
                float _FogEnd;
            CBUFFER_END

            Varyings Vert(Attributes input) {
                Varyings o;
                o.position = TransformObjectToHClip(input.position.xyz);
                o.uv = input.uv;
                o.normal = TransformObjectToWorldNormal(input.normal);
                o.worldPos = TransformObjectToWorld(input.position.xyz);
                return o;
            }

            half4 Frag(Varyings input) : SV_Target {
                half3 albedo = _Color.rgb;
                Light mainLight = GetMainLight();
                half NdotL = saturate(dot(normalize(input.normal), mainLight.direction));
                half3 lighting = albedo * mainLight.color.rgb * NdotL * mainLight.distanceAttenuation + albedo * SampleSH(input.normal);

                float dist = length(input.worldPos - _WorldSpaceCameraPos);
                float fogFactor = saturate((_FogEnd - dist) / max(0.001, _FogEnd - _FogStart));
                fogFactor = pow(fogFactor, _FogDensity * 50.0);

                half3 finalColor = lerp(_FogColor.rgb, lighting, fogFactor);
                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}
