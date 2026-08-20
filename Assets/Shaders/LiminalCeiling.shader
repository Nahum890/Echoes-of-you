Shader "Echoes/LiminalCeiling" {
    Properties {
        _BaseColor ("Panel Color", Color) = (0.78, 0.77, 0.74, 1)
        _GridColor ("Grid Color", Color) = (0.32, 0.31, 0.29, 1)
        _CeilingTex ("Ceiling Albedo", 2D) = "white" {}
        _StainStrength ("Moisture Stain", Range(0,1)) = 0.3
        _StainScale ("Stain Scale", Float) = 5.0
        _FluorescentGlow ("Fluorescent Glow", Range(0,1)) = 0.35
        _Flicker ("Tube Flicker", Range(0,1)) = 0.12
        _PanelCount ("Panel Count per Meter", Float) = 1.0
        _FogDensity ("Fog Density", Float) = 0.01
        _FogColor ("Fog Color", Color) = (0.13, 0.135, 0.14, 1)
        _Smoothness ("Smoothness", Range(0,1)) = 0.05
        _SpecularAnomaly ("Anomalous Specular", Range(0,1)) = 0.08
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
            #pragma shader_feature_local _EMISSION
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHTS_VERTEX
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes { float4 position : POSITION; float2 uv : TEXCOORD0; float3 normal : NORMAL; };
            struct Varyings { float4 position : SV_POSITION; float2 uv : TEXCOORD0; float3 normal : TEXCOORD1; float3 worldPos : TEXCOORD2; float3 viewDir : TEXCOORD3; };

            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor; float4 _GridColor; float4 _FogColor; float4 _EmissionColor;
            float _StainStrength; float _StainScale; float _FluorescentGlow; float _Flicker;
            float _PanelCount; float _FogDensity; float _Smoothness; float _SpecularAnomaly;
            CBUFFER_END

            TEXTURE2D(_CeilingTex); SAMPLER(sampler_CeilingTex);

            float hash(float2 p) { return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453); }
            float noise(float2 p) { float2 i = floor(p); float2 f = frac(p); f = f * f * (3.0 - 2.0 * f); return lerp(lerp(hash(i), hash(i + float2(1,0)), f.x), lerp(hash(i + float2(0,1)), hash(i + float2(1,1)), f.x), f.y); }
            float fbm(float2 p, int octaves) { float v = 0, a = 0.5; for (int i = 0; i < octaves; i++) { v += a * noise(p); p *= 2.0; a *= 0.5; } return v; }

            Varyings Vert(Attributes input) {
                Varyings o;
                o.position = TransformObjectToHClip(input.position.xyz);
                o.uv = input.uv;
                o.normal = TransformObjectToWorldNormal(input.normal);
                o.worldPos = TransformObjectToWorld(input.position.xyz);
                o.viewDir = normalize(_WorldSpaceCameraPos - o.worldPos);
                return o;
            }

            half4 Frag(Varyings input) : SV_Target {
                half3 albedo = _BaseColor.rgb * SAMPLE_TEXTURE2D(_CeilingTex, sampler_CeilingTex, input.uv).rgb;
                float2 gridUV = input.uv * _PanelCount;
                float gx = abs(frac(gridUV.x - 0.5) - 0.5);
                float gy = abs(frac(gridUV.y - 0.5) - 0.5);
                float gridLine = saturate(1.0 - (max(gx, gy) * 60.0));
                albedo = lerp(albedo, _GridColor.rgb, gridLine * 0.85);

                float stain = fbm(input.worldPos.xz * _StainScale + 1.7, 4);
                float stainMask = saturate((stain - 0.55) * 3.0);
                albedo = lerp(albedo, albedo * 0.7 + half3(0.04, 0.06, 0.08), stainMask * _StainStrength);

                Light mainLight = GetMainLight();
                half3 normal = normalize(input.normal);
                half NdotL = max(0, dot(normal, mainLight.direction));
                half3 shAmbient = SampleSH(normal);
                half3 ambient = max(shAmbient, half3(0.6, 0.6, 0.61)) * 1.0;
                half3 directional = mainLight.color.rgb * NdotL * mainLight.distanceAttenuation * mainLight.shadowAttenuation * 0.6;
                half3 lighting = albedo * (ambient + directional);

                half NdotV = max(0, dot(normal, normalize(input.viewDir)));
                float flicker = 1.0 - _Flicker * (0.5 + 0.5 * sin(_Time.y * 41.0 + input.worldPos.x * 7.0));
                half3 glow = half3(0.9, 0.93, 1.0) * _FluorescentGlow * flicker * mainLight.color.rgb;
                lighting += glow;

                // Specular sutil en paneles
                half3 viewDir = normalize(input.viewDir);
                half3 halfDir = normalize(mainLight.direction + viewDir);
                half NdotH = max(0, dot(normal, halfDir));
                half specExp = lerp(80.0, 12.0, _Smoothness);
                half spec = pow(NdotH, specExp) * _Smoothness * _SpecularAnomaly;
                lighting += half3(0.85, 0.88, 0.95) * mainLight.color.rgb * spec * 0.3;

                // Luces adicionales — los techos están bajo los tubos fluorescentes
                uint addCount = GetAdditionalLightsCount();
                for (uint i = 0; i < addCount; i++)
                {
                    Light addLight = GetAdditionalLight(i, input.worldPos);
                    half aNdotL = max(0, dot(normal, addLight.direction));
                    lighting += albedo * addLight.color.rgb * aNdotL * addLight.distanceAttenuation * addLight.shadowAttenuation * 0.55;
                    half3 aHalf = normalize(addLight.direction + viewDir);
                    half aNdotH = max(0, dot(normal, aHalf));
                    half aSpec = pow(aNdotH, specExp) * _Smoothness * _SpecularAnomaly;
                    lighting += half3(0.85, 0.88, 0.95) * addLight.color.rgb * aSpec * 0.35;
                }

                return half4(lighting + _EmissionColor.rgb, 1.0);
            }
            ENDHLSL
        }
        Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On ZTest LEqual ColorMask 0 Cull Off
            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            struct SAttr { float4 position : POSITION; float3 normal : NORMAL; };
            struct SVar { float4 positionCS : SV_POSITION; };
            SVar ShadowVert(SAttr input) {
                SVar o;
                float3 worldPos = TransformObjectToWorld(input.position.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normal);
                float4 clipPos = TransformWorldToHClip(ApplyShadowBias(worldPos, normalWS, _MainLightPosition.xyz));
                #if UNITY_REVERSED_Z
                    clipPos.z = min(clipPos.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    clipPos.z = max(clipPos.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                o.positionCS = clipPos;
                return o;
            }
            half4 ShadowFrag(SVar input) : SV_Target { return 0; }
            ENDHLSL
        }
        Pass {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On ColorMask 0 Cull Off
            HLSLPROGRAM
            #pragma vertex DepthVert
            #pragma fragment DepthFrag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct DAttr { float4 position : POSITION; };
            struct DVar { float4 positionCS : SV_POSITION; };
            DVar DepthVert(DAttr input) { DVar o; o.positionCS = TransformObjectToHClip(input.position.xyz); return o; }
            half4 DepthFrag(DVar input) : SV_Target { return 0; }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}