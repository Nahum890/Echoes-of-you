Shader "Echoes/EchoLiminal" {
    Properties {
        _BaseColor ("Base Color", Color) = (0.0, 0.7, 0.9, 0.45)
        _DistortionStrength ("Space Distortion", Range(0,0.3)) = 0.12
        _ChromaticAberration ("Local CA", Range(0,0.08)) = 0.025
        _DepthOffset ("Depth Sort Offset", Range(-0.5,0.5)) = -0.08
        _ScanlineFreq ("Scanline Freq", Float) = 40
        _ScanlineSpeed ("Scanline Speed", Float) = 2.0
        _TemporalJitter ("Temporal Vertex Jitter", Range(0,0.03)) = 0.008
        _ResonanceGlow ("Resonance Glow", Color) = (0.0, 0.8, 1.0, 0.3)
        _DitherStrength ("Dither Strength", Range(0,1)) = 0.5
        _FifteenFPSCap ("15 FPS Cap", Float) = 1.0
    }
    SubShader {
        Tags { "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" }
        Pass {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes {
                float4 position : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct Varyings {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float _DistortionStrength;
            float _ChromaticAberration;
            float _DepthOffset;
            float _ScanlineFreq;
            float _ScanlineSpeed;
            float _TemporalJitter;
            float4 _ResonanceGlow;
            float _DitherStrength;
            float _FifteenFPSCap;
            CBUFFER_END

            float bayer4x4(float2 pos) {
                float2 b = frac(pos * 0.25) * 4.0;
                float v = 0.0;
                if (b.x > 1.0) v += 1.0;
                if (b.y > 1.0) v += 2.0;
                if (b.x > 2.0) v += 4.0;
                if (b.y > 2.0) v += 8.0;
                return (v + 0.5) / 16.0;
            }

            Varyings Vert(Attributes input) {
                Varyings o;
                float3 pos = input.position.xyz;
                float t = floor(_Time.y * 15.0 * _FifteenFPSCap) / 15.0;
                float jitter = sin(t * 100.0 + input.position.x * 100.0 + input.position.y * 100.0) * _TemporalJitter;
                pos += input.normal * jitter;
                pos += input.normal * sin(_Time.y * 2.0 + input.position.x * 5.0) * _DistortionStrength * 0.1;
                
                float4 clipPos = TransformObjectToHClip(pos);
                clipPos.z += _DepthOffset;
                o.position = clipPos;
                o.uv = input.uv;
                o.normal = TransformObjectToWorldNormal(input.normal);
                o.worldPos = TransformObjectToWorld(input.position.xyz);
                return o;
            }

            half4 Frag(Varyings input) : SV_Target {
                half3 col = _BaseColor.rgb;
                col.r += _ChromaticAberration * input.normal.x;
                col.b -= _ChromaticAberration * input.normal.y;

                half scan = frac(input.worldPos.y * _ScanlineFreq + _Time.y * _ScanlineSpeed);
                col *= lerp(1.0, 0.6, step(0.5, scan));
                col += _ResonanceGlow.rgb * (0.5 + 0.5 * sin(_Time.y * 3.0 + input.worldPos.y * 5.0));

                float dither = bayer4x4(input.position.xy);
                half alpha = _BaseColor.a * (1.0 - _DitherStrength * (1.0 - dither));
                return half4(col, alpha);
            }
            ENDHLSL
        }
    }
}
