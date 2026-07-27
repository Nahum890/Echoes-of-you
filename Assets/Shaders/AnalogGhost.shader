Shader "Echoes/AnalogGhost" {
    Properties {
        _Color ("Base Color", Color) = (0.31, 0.765, 0.91, 0.45)
        _EmissionColor ("Emission Color", Color) = (0.0, 0.5, 0.65, 1.0)
        _ScanlineDensity ("Scanline Density", Float) = 80.0
        _ScanlineSpeed ("Scanline Speed", Float) = 2.0
        _ScanlineThickness ("Scanline Thickness", Float) = 0.5
        _FPS ("FPS Cap", Float) = 15.0
    }
    SubShader {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        LOD 200

        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }
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
                float3 worldPos : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _EmissionColor;
                float _ScanlineDensity;
                float _ScanlineSpeed;
                float _ScanlineThickness;
                float _FPS;
            CBUFFER_END

            static const float4x4 kBayer4x4 = float4x4(
                0.0 / 16.0,  8.0 / 16.0,  2.0 / 16.0, 10.0 / 16.0,
               12.0 / 16.0,  4.0 / 16.0, 14.0 / 16.0,  6.0 / 16.0,
                3.0 / 16.0, 11.0 / 16.0,  1.0 / 16.0,  9.0 / 16.0,
               15.0 / 16.0,  7.0 / 16.0, 13.0 / 16.0,  5.0 / 16.0
            );

            float Bayer4x4(float2 screenUV) {
                uint x = uint(fmod(screenUV.x, 4.0));
                uint y = uint(fmod(screenUV.y, 4.0));
                return kBayer4x4[x][y];
            }

            Varyings Vert(Attributes input) {
                Varyings o;
                float3 objectPos = input.position.xyz;
                float3 worldPos = TransformObjectToWorld(objectPos);
                
                o.position = TransformObjectToHClip(objectPos);
                o.uv = input.uv;
                o.worldPos = worldPos;
                o.screenPos = ComputeScreenPos(o.position);
                return o;
            }

            half4 Frag(Varyings input) : SV_Target {
                float2 screenUV = (input.screenPos.xy / input.screenPos.w) * _ScreenParams.xy;
                float dither = Bayer4x4(screenUV);

                half alpha = _Color.a;
                if (alpha < dither * 0.9) {
                    discard;
                }

                float scan = frac(input.worldPos.y * _ScanlineDensity + _Time.y * _ScanlineSpeed);
                if (scan < _ScanlineThickness) {
                    alpha *= 0.5;
                }

                float pulse = 0.85 + 0.15 * sin(_Time.y * 4.0);
                half3 emission = _EmissionColor.rgb * pulse;

                half3 finalColor = _Color.rgb + emission;
                return half4(finalColor, alpha);
            }
            ENDHLSL
        }

        Pass {
            Name "UniversalForwardOnly"
            Tags { "LightMode" = "UniversalForwardOnly" }
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
                float3 worldPos : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _EmissionColor;
                float _ScanlineDensity;
                float _ScanlineSpeed;
                float _ScanlineThickness;
                float _FPS;
            CBUFFER_END

            static const float4x4 kBayer4x4 = float4x4(
                0.0 / 16.0,  8.0 / 16.0,  2.0 / 16.0, 10.0 / 16.0,
               12.0 / 16.0,  4.0 / 16.0, 14.0 / 16.0,  6.0 / 16.0,
                3.0 / 16.0, 11.0 / 16.0,  1.0 / 16.0,  9.0 / 16.0,
               15.0 / 16.0,  7.0 / 16.0, 13.0 / 16.0,  5.0 / 16.0
            );

            float Bayer4x4(float2 screenUV) {
                uint x = uint(fmod(screenUV.x, 4.0));
                uint y = uint(fmod(screenUV.y, 4.0));
                return kBayer4x4[x][y];
            }

            Varyings Vert(Attributes input) {
                Varyings o;
                float3 objectPos = input.position.xyz;
                float3 worldPos = TransformObjectToWorld(objectPos);
                
                o.position = TransformObjectToHClip(objectPos);
                o.uv = input.uv;
                o.worldPos = worldPos;
                o.screenPos = ComputeScreenPos(o.position);
                return o;
            }

            half4 Frag(Varyings input) : SV_Target {
                float2 screenUV = (input.screenPos.xy / input.screenPos.w) * _ScreenParams.xy;
                float dither = Bayer4x4(screenUV);

                half alpha = _Color.a;
                if (alpha < dither * 0.9) {
                    discard;
                }

                float scan = frac(input.worldPos.y * _ScanlineDensity + _Time.y * _ScanlineSpeed);
                if (scan < _ScanlineThickness) {
                    alpha *= 0.5;
                }

                float pulse = 0.85 + 0.15 * sin(_Time.y * 4.0);
                half3 emission = _EmissionColor.rgb * pulse;

                half3 finalColor = _Color.rgb + emission;
                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}
