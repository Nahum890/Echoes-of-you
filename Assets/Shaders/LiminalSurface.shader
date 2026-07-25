Shader "Echoes/LiminalSurface" {
    Properties {
        _BaseColor ("Base Color", Color) = (0.1, 0.12, 0.15, 1)
        _BaseTex ("Albedo", 2D) = "white" {}
        _FresnelInvert ("Inverted Fresnel", Range(0,1)) = 0.3
        _SubsurfaceTint ("Subsurface Tint", Color) = (0.0, 0.08, 0.12, 1)
        _FluorescentEdge ("Fluorescent Edge", Range(0,1)) = 0.12
        _StainNoiseScale ("Stain Noise Scale", Float) = 4.0
        _StainThreshold ("Stain Threshold", Range(0,1)) = 0.7
        _StainColor ("Stain Color", Color) = (0.05, 0.08, 0.12, 1)
        _WearNoiseScale ("Wear Noise Scale", Float) = 8.0
        _WearHeight ("Wear Height (world Y)", Float) = 1.5
        _WearColor ("Wear Color", Color) = (0.08, 0.1, 0.14, 1)
        _SpecularAnomaly ("Anomalous Specular", Range(0,1)) = 0.15
        _DepthDistort ("Vertex Depth Distort", Range(0,0.02)) = 0.005
        _EmissionColor ("Emission", Color) = (0,0,0,0)
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
            #pragma shader_feature _EMISSION
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes { float4 position : POSITION; float2 uv : TEXCOORD0; float3 normal : NORMAL; };
            struct Varyings { float4 position : SV_POSITION; float2 uv : TEXCOORD0; float3 normal : TEXCOORD1; float3 worldPos : TEXCOORD2; float3 viewDir : TEXCOORD3; };

            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor; float4 _SubsurfaceTint; float4 _StainColor; float4 _WearColor; float4 _EmissionColor;
            float _FresnelInvert; float _FluorescentEdge; float _StainNoiseScale; float _StainThreshold;
            float _WearNoiseScale; float _WearHeight; float _SpecularAnomaly; float _DepthDistort;
            CBUFFER_END

            TEXTURE2D(_BaseTex); SAMPLER(sampler_BaseTex);

            float hash(float2 p) { return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453); }
            float noise(float2 p) { float2 i = floor(p); float2 f = frac(p); f = f * f * (3.0 - 2.0 * f); return lerp(lerp(hash(i), hash(i + float2(1,0)), f.x), lerp(hash(i + float2(0,1)), hash(i + float2(1,1)), f.x), f.y); }
            float fbm(float2 p, int octaves) { float v = 0, a = 0.5; for (int i = 0; i < octaves; i++) { v += a * noise(p); p *= 2.0; a *= 0.5; } return v; }

            Varyings Vert(Attributes input) {
                Varyings o;
                float3 worldPos = TransformObjectToWorld(input.position.xyz);
                worldPos.y += sin(worldPos.x * 10.0 + worldPos.z * 10.0 + _Time.y * 0.3) * _DepthDistort;
                o.position = TransformWorldToHClip(worldPos);
                o.uv = input.uv; o.normal = TransformObjectToWorldNormal(input.normal);
                o.worldPos = worldPos; o.viewDir = normalize(_WorldSpaceCameraPos - worldPos);
                return o;
            }

            half4 Frag(Varyings input) : SV_Target {
                half3 albedo = _BaseColor.rgb * SAMPLE_TEXTURE2D(_BaseTex, sampler_BaseTex, input.uv).rgb;
                float stain = fbm(input.uv * _StainNoiseScale + input.worldPos.xz * 0.1, 4);
                if (stain > _StainThreshold) albedo = lerp(albedo, _StainColor.rgb, (stain - _StainThreshold) / max(0.001, (1.0 - _StainThreshold)));
                float wear = 0; if (input.worldPos.y < _WearHeight) { wear = fbm(input.uv * _WearNoiseScale, 3); if (wear > 0.6) albedo = lerp(albedo, _WearColor.rgb, (wear - 0.6) / 0.4); }

                Light mainLight = GetMainLight();
                half3 normal = normalize(input.normal); half3 viewDir = normalize(input.viewDir);
                half NdotL = max(0, dot(normal, mainLight.direction));
                half3 lighting = albedo * mainLight.color.rgb * NdotL * mainLight.distanceAttenuation + albedo * SampleSH(normal) * 0.3;
                half NdotV = max(0, dot(normal, input.viewDir));
                half fresnelInv = pow(1.0 - NdotV, 2.0) * _FresnelInvert;
                lighting += mainLight.color.rgb * fresnelInv * 0.5;
                half edgeGlow = step(0.93, NdotV) * _FluorescentEdge;
                lighting += half3(0.8, 0.9, 1.0) * edgeGlow * mainLight.color.rgb * 0.8;
                lighting += _SubsurfaceTint.rgb * (1.0 - NdotV) * mainLight.color.rgb * 0.1;
                half3 anomalousSpec = mainLight.color.rgb * _SpecularAnomaly * pow(fresnelInv, 3);
                lighting += anomalousSpec;
                half3 emission = _EmissionColor.rgb;
                return half4(lighting + emission, _BaseColor.a);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}
