Shader "Echoes/PS1World" {
    Properties {
        _BaseColor ("Base Color", Color) = (0.1, 0.12, 0.15, 1)
        _BaseTex ("Albedo", 2D) = "white" {}
        _VertexSnap ("Vertex Snap (world units)", Float) = 16.0
        _DepthJitter ("Depth Vertex Jitter", Range(0,0.01)) = 0.002
        _AffineStrength ("Affine Mapping Strength", Range(0,1)) = 0.5
        _ScanlineFreq ("Scanline Frequency", Float) = 120.0
        _DitherStrength ("Bayer Dither", Range(0,1)) = 0.6
        _QuantizeColors ("Color Quantization Steps", Float) = 64.0
        _FogDensity ("Fog Density", Float) = 0.008
        _FogColor ("Fog Color", Color) = (0.04, 0.04, 0.05, 1)
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
            float4 _BaseColor; float4 _EmissionColor; float _VertexSnap; float _DepthJitter;
            float _AffineStrength; float _ScanlineFreq; float _DitherStrength; float _QuantizeColors;
            float _FogDensity; float4 _FogColor;
            TEXTURE2D(_BaseTex); SAMPLER(sampler_BaseTex);
            CBUFFER_END

            float bayer4x4(float2 pos) { float2 b = frac(pos * 0.25) * 4.0; float v = 0.0; if (b.x > 1.0) v += 1.0; if (b.y > 1.0) v += 2.0; if (b.x > 2.0) v += 4.0; if (b.y > 2.0) v += 8.0; return (v + 0.5) / 16.0; }

            Varyings Vert(Attributes input) {
                Varyings o;
                float3 pos = input.position.xyz;
                // Vertex snapping a grid PS1
                pos = floor(pos * _VertexSnap) / _VertexSnap;
                // Near-plane jitter (swim)
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
                // Affine mapping simulation: distort UV by 1/w
                float invW = 1.0 / max(0.001, input.clipPos.w);
                float2 affineUV = input.uv * invW * _AffineStrength + input.uv * (1.0 - _AffineStrength);
                albedo = _BaseColor.rgb * SAMPLE_TEXTURE2D(_BaseTex, sampler_BaseTex, affineUV).rgb;
                // Scanlines
                half scan = frac(input.worldPos.y * _ScanlineFreq + _Time.y * 2.0);
                albedo *= lerp(1.0, 0.5, step(0.5, scan));
                // Lighting
                Light mainLight = GetMainLight();
                half3 normal = normalize(input.normal);
                half NdotL = max(0, dot(normal, mainLight.direction));
                half3 lighting = albedo * mainLight.color.rgb * NdotL * mainLight.distanceAttenuation;
                // Fog
                float fog = 1.0 - exp(-_FogDensity * length(input.worldPos - _WorldSpaceCameraPos));
                lighting = lerp(lighting, _FogColor.rgb, fog);
                // Color quantization (64 colors)
                lighting = floor(lighting * _QuantizeColors) / _QuantizeColors;
                // Bayer dither alpha
                float dither = bayer4x4(input.position.xy);
                half alpha = _BaseColor.a * (1.0 - _DitherStrength * (1.0 - dither));
                return half4(lighting + _EmissionColor.rgb, alpha);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}
