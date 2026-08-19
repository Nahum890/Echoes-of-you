Shader "Echoes/LiminalSurface" {
    Properties {
        _BaseColor ("Base Color", Color) = (0.42, 0.44, 0.46, 1)
        _BaseTex ("Albedo", 2D) = "white" {}
        _DetailTex ("Detail Stain", 2D) = "white" {}
        [Header(Liminal Institutional Lighting)]
        _FlatAmbient ("Flat Ambient Strength", Range(0,2)) = 1.2
        _DirectionalScale ("Directional Scale", Range(0,1)) = 0.35
        _FluorescentHum ("Fluorescent Hum", Range(0,1)) = 0.4
        _FlickerStrength ("Tube Flicker", Range(0,1)) = 0.18
        [Header(Fresnel and Edge)]
        _FresnelInvert ("Inverted Fresnel", Range(0,1)) = 0.45
        _FluorescentEdge ("Fluorescent Edge", Range(0,1)) = 0.22
        _SubsurfaceTint ("Subsurface Tint", Color) = (0.0, 0.08, 0.12, 1)
        [Header(Moisture Stains)]
        _StainNoiseScale ("Stain Noise Scale", Float) = 7.0
        _StainThreshold ("Stain Threshold", Range(0,1)) = 0.55
        _StainColor ("Stain Color", Color) = (0.08, 0.10, 0.09, 1)
        _StainStrength ("Stain Strength", Range(0,1)) = 0.7
        [Header(Wear and Cracks)]
        _WearNoiseScale ("Wear Noise Scale", Float) = 12.0
        _WearHeight ("Wear Height (world Y)", Float) = 1.5
        _WearColor ("Wear Color", Color) = (0.06, 0.07, 0.09, 1)
        _CrackThreshold ("Crack Threshold", Range(0.9,0.999)) = 0.992
        _CrackColor ("Crack Color", Color) = (0.02, 0.02, 0.03, 1)
        [Header(Posterization and Fog)]
        _ColorBands ("Color Posterization Bands", Float) = 32.0
        _FogDensity ("Liminal Fog Density", Float) = 0.018
        _FogColor ("Fog Color", Color) = (0.38, 0.39, 0.41, 1)
        [Header(Anomaly)]
        _SpecularAnomaly ("Anomalous Specular", Range(0,1)) = 0.10
        _Smoothness ("Smoothness", Range(0,1)) = 0.15
        _DepthDistort ("Vertex Depth Distort", Range(0,0.02)) = 0.003
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
            struct Varyings { float4 position : SV_POSITION; float2 uv : TEXCOORD0; float3 normal : TEXCOORD1; float3 worldPos : TEXCOORD2; float3 viewDir : TEXCOORD3; float4 clipPos : TEXCOORD4; };

            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor; float4 _SubsurfaceTint; float4 _StainColor; float4 _WearColor; float4 _EmissionColor; float4 _CrackColor; float4 _FogColor;
            float _FlatAmbient; float _DirectionalScale; float _FluorescentHum; float _FlickerStrength;
            float _FresnelInvert; float _FluorescentEdge;
            float _StainNoiseScale; float _StainThreshold; float _StainStrength;
            float _WearNoiseScale; float _WearHeight; float _CrackThreshold;
            float _ColorBands; float _FogDensity; float _SpecularAnomaly; float _Smoothness; float _DepthDistort;
            CBUFFER_END

            TEXTURE2D(_BaseTex); SAMPLER(sampler_BaseTex);
            TEXTURE2D(_DetailTex); SAMPLER(sampler_DetailTex);

            float hash(float2 p) { return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453); }
            float noise(float2 p) { float2 i = floor(p); float2 f = frac(p); f = f * f * (3.0 - 2.0 * f); return lerp(lerp(hash(i), hash(i + float2(1,0)), f.x), lerp(hash(i + float2(0,1)), hash(i + float2(1,1)), f.x), f.y); }
            float fbm(float2 p, int octaves) { float v = 0, a = 0.5; for (int i = 0; i < octaves; i++) { v += a * noise(p); p *= 2.0; a *= 0.5; } return v; }

            Varyings Vert(Attributes input) {
                Varyings o;
                float3 worldPos = TransformObjectToWorld(input.position.xyz);
                worldPos.y += sin(worldPos.x * 8.0 + worldPos.z * 8.0 + _Time.y * 0.25) * _DepthDistort;
                float4 clipPos = TransformWorldToHClip(worldPos);
                o.position = clipPos; o.clipPos = clipPos;
                o.uv = input.uv; o.normal = TransformObjectToWorldNormal(input.normal);
                o.worldPos = worldPos; o.viewDir = normalize(_WorldSpaceCameraPos - worldPos);
                return o;
            }

            half4 Frag(Varyings input) : SV_Target {
                half3 albedo = _BaseColor.rgb * SAMPLE_TEXTURE2D(_BaseTex, sampler_BaseTex, input.uv).rgb;

                // Moisture stains — más prominentes en zonas bajas (cerca del suelo) y en paredes
                float2 stainUV = input.uv * _StainNoiseScale + input.worldPos.xz * 0.08;
                float stain = fbm(stainUV, 5);
                float heightFactor = saturate(1.0 - input.worldPos.y / 3.0); // más manchas cerca del suelo
                if (stain > _StainThreshold) {
                    float stainAmt = (stain - _StainThreshold) / max(0.001, (1.0 - _StainThreshold));
                    albedo = lerp(albedo, _StainColor.rgb, stainAmt * _StainStrength * (0.4 + 0.6 * heightFactor));
                }

                // Wear (desgaste por fricción/arrastre)
                float wear = 0;
                if (input.worldPos.y < _WearHeight) {
                    wear = fbm(input.uv * _WearNoiseScale + input.worldPos.xz * 0.05, 4);
                    if (wear > 0.55) albedo = lerp(albedo, _WearColor.rgb, (wear - 0.55) / 0.45 * 0.6);
                }

                // Grietas procedurales (más finas y extendidas)
                float crack = pow(fbm(input.uv * 50.0 + input.worldPos.xz * 0.3, 4), 5.0);
                if (crack > _CrackThreshold) albedo = lerp(albedo, _CrackColor.rgb, (crack - _CrackThreshold) / (1.0 - _CrackThreshold));

                // Detail texture overlay
                half3 detail = SAMPLE_TEXTURE2D(_DetailTex, sampler_DetailTex, input.uv * 3.0).rgb;
                albedo = lerp(albedo, albedo * detail, 0.3);

                Light mainLight = GetMainLight();
                half3 normal = normalize(input.normal);
                half3 viewDir = normalize(input.viewDir);
                half NdotL = max(0, dot(normal, mainLight.direction));
                half NdotV = max(0.001, dot(normal, viewDir));

                // Ambient from SH + unlit floor (mínimo de iluminación institutional difusa)
                half3 shAmbient = SampleSH(normal);
                half ambientFloor = 0.35; // para que nunca se vea negro total ni plano
                half3 ambient = max(shAmbient, half3(ambientFloor, ambientFloor, ambientFloor * 1.02)) * _FlatAmbient;

                // Luz direccional con sombras suaves (matado pero presente para dar forma)
                half3 directional = mainLight.color.rgb * NdotL * mainLight.distanceAttenuation * mainLight.shadowAttenuation * _DirectionalScale;

                // Componente superior (luz del techo simulada — fresnel de techo)
                half upFacing = max(0, normal.y);
                half3 ceilingLight = half3(0.85, 0.88, 0.95) * upFacing * 0.35;
                half3 lighting = albedo * (ambient + directional + ceilingLight);

                // Fluorescent hum parpadeante (sutil)
                float flicker = 1.0 - _FlickerStrength * (0.5 + 0.5 * sin(_Time.y * 47.0 + input.worldPos.x * 11.0 + input.worldPos.z * 13.0));
                half3 fluoHum = half3(0.7, 0.75, 0.85) * _FluorescentHum * flicker;
                lighting += albedo * fluoHum * 0.3;

                // Fresnel suave (ligero oscurecimiento en ángulos rasantes)
                half fresnelInv = pow(abs(1.0 - NdotV), 2.5) * _FresnelInvert;
                lighting -= mainLight.color.rgb * fresnelInv * 0.10;

                // Edge glow fluorescente (resalta bordes institucionales)
                half edgeGlow = smoothstep(0.88, 0.98, NdotV) * _FluorescentEdge;
                lighting += half3(0.75, 0.82, 0.92) * edgeGlow * mainLight.color.rgb * 0.6 * flicker;

                // Specular anómalo — variación de roughness por material
                half3 halfDir = normalize(mainLight.direction + viewDir);
                half NdotH = max(0, dot(normal, halfDir));
                half specExp = lerp(64.0, 8.0, _Smoothness);
                half spec = pow(NdotH, specExp) * _Smoothness * _SpecularAnomaly;
                lighting += half3(0.85, 0.88, 0.95) * mainLight.color.rgb * spec * 0.4;

                half3 emission = _EmissionColor.rgb;
                return half4(lighting + emission, _BaseColor.a);
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
            CBUFFER_START(UnityPerMaterial)
            float _DepthDistort;
            CBUFFER_END
            struct SAttr { float4 position : POSITION; float3 normal : NORMAL; };
            struct SVar { float4 positionCS : SV_POSITION; };
            float3 TransformObjectToWorldNormalSafe(float3 n) { return normalize(mul((float3x3)unity_ObjectToWorld, n)); }
            SVar ShadowVert(SAttr input) {
                SVar o;
                float3 worldPos = TransformObjectToWorld(input.position.xyz);
                float3 normalWS = TransformObjectToWorldNormalSafe(input.normal);
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
            CBUFFER_START(UnityPerMaterial)
            float _DepthDistort;
            CBUFFER_END
            struct DAttr { float4 position : POSITION; };
            struct DVar { float4 positionCS : SV_POSITION; };
            DVar DepthVert(DAttr input) {
                DVar o;
                o.positionCS = TransformObjectToHClip(input.position.xyz);
                return o;
            }
            half4 DepthFrag(DVar input) : SV_Target { return 0; }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}