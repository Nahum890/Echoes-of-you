Shader "Echoes/LiminalFogVolume" {
    Properties {
        _FogColor ("Fog Color", Color) = (0.11, 0.14, 0.19, 1)
        _FogDensity ("Fog Density", Float) = 0.02
        _CornerAccumulation ("Corner Accumulation", Range(0,1)) = 0.35
        _LightScatter ("Light Scatter", Range(0,1)) = 0.25
        _NoiseScale ("Fog Noise Scale", Float) = 0.5
        _NoiseSpeed ("Fog Noise Speed", Float) = 0.05
        _FogStart ("Fog Start", Float) = 0.0
        _FogEnd ("Fog End", Float) = 20.0
    }
    SubShader {
        Tags { "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" }
        Pass {
            Name "FogPass"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct Attributes { float4 position : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 position : SV_POSITION; float2 uv : TEXCOORD0; float4 screenPos : TEXCOORD1; };

            CBUFFER_START(UnityPerMaterial)
            float4 _FogColor;
            float _FogDensity;
            float _FogStart;
            float _FogEnd;
            float _CornerAccumulation;
            float _LightScatter;
            float _NoiseScale;
            float _NoiseSpeed;
            CBUFFER_END

            float hash3d(float3 p) {
                p = frac(p * float3(0.1031, 0.1030, 0.0973));
                p += dot(p, p.yzx + 33.33);
                return frac((p.x + p.y) * p.z);
            }

            float noise3d(float3 p) {
                float3 i = floor(p);
                float3 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(
                    lerp(lerp(hash3d(i), hash3d(i + float3(1,0,0)), f.x),
                         lerp(hash3d(i + float3(0,1,0)), hash3d(i + float3(1,1,0)), f.x), f.y),
                    lerp(lerp(hash3d(i + float3(0,0,1)), hash3d(i + float3(1,0,1)), f.x),
                         lerp(hash3d(i + float3(0,1,1)), hash3d(i + float3(1,1,1)), f.x), f.y), f.z);
            }

            float fbm3d(float3 p) {
                float v = 0.0, a = 0.5;
                for (int i = 0; i < 3; i++) {
                    v += a * noise3d(p);
                    p *= 2.0;
                    a *= 0.5;
                }
                return v;
            }

            Varyings Vert(Attributes input) {
                Varyings o;
                o.position = TransformObjectToHClip(input.position.xyz);
                o.uv = input.uv;
                o.screenPos = ComputeScreenPos(o.position);
                return o;
            }

            half4 Frag(Varyings input) : SV_Target {
                float2 uv = input.screenPos.xy / max(0.0001, input.screenPos.w);
                float rawDepth = SampleSceneDepth(uv);
                float3 worldPos = ComputeWorldSpacePosition(uv, rawDepth, UNITY_MATRIX_I_VP);

                float heightFactor = saturate((_FogEnd - worldPos.y) / max(0.001, (_FogEnd - _FogStart)));
                float density = _FogDensity * heightFactor;

                float depthRight = SampleSceneDepth(uv + float2(0.001, 0));
                float depthUp = SampleSceneDepth(uv + float2(0, 0.001));
                float cornerAcc = length(float2(depthRight - rawDepth, depthUp - rawDepth)) * _CornerAccumulation * 10.0;

                float fogNoise = fbm3d(worldPos * _NoiseScale + float3(0, _Time.y * _NoiseSpeed, 0));
                float scatter = _LightScatter * 0.1;
                float totalDensity = saturate(density + cornerAcc + fogNoise * 0.3 + scatter);

                half3 color = _FogColor.rgb;
                return half4(color, totalDensity);
            }
            ENDHLSL
        }
    }
}
