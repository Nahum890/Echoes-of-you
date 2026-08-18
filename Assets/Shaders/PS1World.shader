Shader "Echoes/PS1World" {
    Properties {
        _BaseColor ("Base Color", Color) = (0.32, 0.34, 0.36, 1)
        _BaseTex ("Albedo", 2D) = "white" {}
        _VertexSnap ("Vertex Snap (world units)", Float) = 16.0
        _DepthJitter ("Depth Vertex Jitter", Range(0,0.01)) = 0.002
        _AffineStrength ("Affine Mapping Strength", Range(0,1)) = 0.5
        _ScanlineFreq ("Scanline Frequency", Float) = 120.0
        _DitherStrength ("Bayer Dither", Range(0,1)) = 0.6
        _QuantizeColors ("Color Quantization Steps", Float) = 32.0
        _FogDensity ("Fog Density", Float) = 0.016
        _FogColor ("Fog Color", Color) = (0.38, 0.39, 0.41, 1)
        _StainStrength ("Liminal Stain", Range(0,1)) = 0.4
        _StainScale ("Stain Scale", Float) = 6.0
        _FluorescentHum ("Fluorescent Hum", Range(0,1)) = 0.35
        _FlickerStrength ("Tube Flicker", Range(0,1)) = 0.15
        _FlatAmbient ("Flat Ambient", Range(0,2)) = 1.0
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
            float4 _BaseColor; float4 _EmissionColor; float4 _FogColor;
            float _VertexSnap; float _DepthJitter;
            float _AffineStrength; float _ScanlineFreq; float _DitherStrength; float _QuantizeColors;
            float _FogDensity; float _StainStrength; float _StainScale;
            float _FluorescentHum; float _FlickerStrength; float _FlatAmbient;
            CBUFFER_END

            TEXTURE2D(_BaseTex); SAMPLER(sampler_BaseTex);

            float bayer4x4(float2 pos) { float2 b = frac(pos * 0.25) * 4.0; float v = 0.0; if (b.x > 1.0) v += 1.0; if (b.y > 1.0) v += 2.0; if (b.x > 2.0) v += 4.0; if (b.y > 2.0) v += 8.0; return (v + 0.5) / 16.0; }
            float hash2(float2 p) { return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453); }
            float noise2(float2 p) { float2 i = floor(p); float2 f = frac(p); f = f * f * (3.0 - 2.0 * f); return lerp(lerp(hash2(i), hash2(i + float2(1,0)), f.x), lerp(hash2(i + float2(0,1)), hash2(i + float2(1,1)), f.x), f.y); }
            float fbm(float2 p, int octaves) { float v = 0, a = 0.5; for (int i = 0; i < octaves; i++) { v += a * noise2(p); p *= 2.0; a *= 0.5; } return v; }

            Varyings Vert(Attributes input) {
                Varyings o;
                float3 pos = input.position.xyz;
                pos = floor(pos * _VertexSnap) / _VertexSnap;
                float zDepth = length(TransformObjectToWorld(pos) - _WorldSpaceCameraPos);
                float jitter = sin(zDepth * 50.0 + _Time.y * 10.0) * _DepthJitter;
                pos += input.normal * jitter;
                float4 clipPos = TransformObjectToHClip(pos);
                o.position = clipPos;
                o.clipPos = clipPos;
                o.uv = input.uv;
                o.normal = TransformObjectToWorldNormal(input.normal);
                o.worldPos = TransformObjectToWorld(input.position.xyz);
                o.viewDir = normalize(_WorldSpaceCameraPos - o.worldPos);
                return o;
            }

            half4 Frag(Varyings input) : SV_Target {
                half3 albedo = _BaseColor.rgb * SAMPLE_TEXTURE2D(_BaseTex, sampler_BaseTex, input.uv).rgb;
                float invW = 1.0 / max(0.001, input.clipPos.w);
                float2 affineUV = input.uv * invW * _AffineStrength + input.uv * (1.0 - _AffineStrength);
                albedo = _BaseColor.rgb * SAMPLE_TEXTURE2D(_BaseTex, sampler_BaseTex, affineUV).rgb;

                // Liminal moisture stains (procedural, based on world position)
                float stainNoise = fbm(input.worldPos.xz * _StainScale + input.worldPos.y * 0.5, 5);
                float heightFactor = saturate(1.0 - input.worldPos.y / 3.5);
                float stainMask = saturate((stainNoise - 0.5) * 2.5) * saturate(1.0 - abs(input.normal.y));
                albedo = lerp(albedo, albedo * 0.6 + half3(0.05, 0.07, 0.08), stainMask * _StainStrength * (0.4 + 0.6 * heightFactor));

                // Scanlines (PS1 CRT)
                half scan = frac(input.worldPos.y * _ScanlineFreq + _Time.y * 2.0);
                albedo *= lerp(1.0, 0.85, step(0.5, scan));

                Light mainLight = GetMainLight();
                half3 normal = normalize(input.normal);
                half NdotL = max(0, dot(normal, mainLight.direction));

                // Ambient + floor mínimo + SH + luz de techo
                half3 shAmbient = SampleSH(normal);
                half ambientFloor = 0.35;
                half3 ambient = max(shAmbient, half3(ambientFloor, ambientFloor, ambientFloor * 1.02)) * _FlatAmbient;
                half3 directional = mainLight.color.rgb * NdotL * mainLight.distanceAttenuation * mainLight.shadowAttenuation * 0.5;
                half upFacing = max(0, normal.y);
                half3 ceilingLight = half3(0.85, 0.88, 0.95) * upFacing * 0.35;
                half3 lighting = albedo * (ambient + directional + ceilingLight);

                // Fluorescent hum parpadeante (sutil)
                float flicker = 1.0 - _FlickerStrength * (0.5 + 0.5 * sin(_Time.y * 43.0 + input.worldPos.x * 9.0 + input.worldPos.z * 7.0));
                half3 fluoHum = half3(0.7, 0.75, 0.85) * _FluorescentHum * flicker;
                lighting += albedo * fluoHum * 0.25;

                // Color quantization (PS1 — bandas discretas pero suaves)
                lighting = floor(lighting * _QuantizeColors + 0.5) / _QuantizeColors;

                half alpha = _BaseColor.a;
                return half4(lighting + _EmissionColor.rgb, alpha);
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