Shader "Echoes/PS1World"
{
    // Superficies del mundo (arquitectura, props). El look PS1 de framebuffer
    // (dither, cuantizacion de color, scanlines) NO vive aqui: lo aplica la
    // Renderer Feature "Echoes PS1 Post" sobre la imagen entera. Este shader
    // solo se ocupa de lo que es propio de la geometria: snapping de vertices,
    // affine mapping y el sombreado plano liminal.
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.32, 0.34, 0.36, 1)
        _BaseTex ("Albedo", 2D) = "white" {}

        [Header(PS1)]
        _SnapPixelSize ("Rejilla de snap (px internos)", Range(0, 8)) = 2
        _AffineStrength ("Affine Mapping", Range(0,1)) = 0.5

        [Header(Liminal)]
        _StainStrength ("Manchas de humedad", Range(0,1)) = 0.4
        _StainScale ("Escala de manchas", Float) = 6.0
        _FluorescentHum ("Zumbido fluorescente", Range(0,1)) = 0.35
        _FlickerStrength ("Parpadeo del tubo", Range(0,1)) = 0.15
        _FlatAmbient ("Ambiente plano", Range(0,2)) = 1.0
        _AmbientFloor ("Suelo de ambiente", Range(0,1)) = 0.6

        [Header(Surface)]
        _Smoothness ("Smoothness", Range(0,1)) = 0.15
        _SpecColor ("Specular Tint", Color) = (0.9, 0.92, 0.95, 1)
        _SpecularAnomaly ("Specular Strength", Range(0,1)) = 0.25
        [HDR] _EmissionColor ("Emission", Color) = (0,0,0,0)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 200

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "PS1Common.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float4 _EmissionColor;
            float4 _SpecColor;
            float4 _BaseTex_ST;
            float _SnapPixelSize;
            float _AffineStrength;
            float _StainStrength;
            float _StainScale;
            float _FluorescentHum;
            float _FlickerStrength;
            float _FlatAmbient;
            float _AmbientFloor;
            float _Smoothness;
            float _SpecularAnomaly;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            // noperspective (para el affine mapping real) necesita SM4+.
            #pragma target 4.5

            #pragma shader_feature_local _EMISSION

            // Sin estos keywords GetMainLight() devuelve shadowAttenuation = 1
            // siempre: la luz direccional nunca proyectaba sombra sobre estos
            // materiales, que era el bug mas visible del shader anterior.
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseTex);
            SAMPLER(sampler_BaseTex);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                // El affine mapping de PS1 no es una UV escalada: es la MISMA UV
                // interpolada sin correccion de perspectiva. Eso se pide con
                // 'noperspective', no multiplicando por 1/w como se hacia antes.
                noperspective float2 uvAffine : TEXCOORD1;
                float3 normalWS   : TEXCOORD2;
                float3 positionWS : TEXCOORD3;
                float  fogFactor  : TEXCOORD4;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;

                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float4 positionCS = TransformWorldToHClip(positionWS);
                positionCS = PS1SnapClipPos(positionCS, _SnapPixelSize);

                output.positionCS = positionCS;
                output.positionWS = positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseTex);
                output.uvAffine = output.uv;
                output.fogFactor = ComputeFogFactor(positionCS.z);

                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = lerp(input.uv, input.uvAffine, _AffineStrength);
                half3 albedo = _BaseColor.rgb * SAMPLE_TEXTURE2D(_BaseTex, sampler_BaseTex, uv).rgb;

                half3 normalWS = normalize(input.normalWS);
                half3 viewDirWS = normalize(GetWorldSpaceViewDir(input.positionWS));

                // Manchas de humedad procedurales. La guarda evita 12 sin() por
                // pixel en los materiales que no las usan.
                if (_StainStrength > 0.0)
                {
                    float stainNoise = PS1Fbm(input.positionWS.xz * _StainScale + input.positionWS.y * 0.5);
                    float heightFactor = saturate(1.0 - input.positionWS.y / 3.5);
                    float stainMask = saturate((stainNoise - 0.5) * 2.5) * saturate(1.0 - abs(normalWS.y));
                    albedo = lerp(albedo, albedo * 0.6 + half3(0.05, 0.07, 0.08),
                                  stainMask * _StainStrength * (0.4 + 0.6 * heightFactor));
                }

                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                Light mainLight = GetMainLight(shadowCoord);

                half3 shAmbient = SampleSH(normalWS);
                half3 ambient = max(shAmbient, half3(_AmbientFloor, _AmbientFloor, _AmbientFloor * 1.02)) * _FlatAmbient;

                half NdotL = saturate(dot(normalWS, mainLight.direction));
                half3 directional = mainLight.color.rgb * NdotL *
                                    mainLight.distanceAttenuation * mainLight.shadowAttenuation * 0.6;

                float2 screenUV = GetNormalizedScreenSpaceUV(input.positionCS);

                #if defined(_SCREEN_SPACE_OCCLUSION)
                    AmbientOcclusionFactor aoFactor = GetScreenSpaceAmbientOcclusion(screenUV);
                    ambient *= aoFactor.indirectAmbientOcclusion;
                    directional *= aoFactor.directAmbientOcclusion;
                #endif

                half upFacing = saturate(normalWS.y);
                half3 ceilingLight = half3(0.9, 0.92, 1.0) * upFacing * 0.42;
                half3 lighting = albedo * (ambient + directional + ceilingLight);

                // Zumbido fluorescente parpadeante.
                float flicker = 1.0 - _FlickerStrength *
                    (0.5 + 0.5 * sin(_Time.y * 43.0 + input.positionWS.x * 9.0 + input.positionWS.z * 7.0));
                lighting += albedo * half3(0.7, 0.75, 0.85) * _FluorescentHum * flicker * 0.25;

                half specExp = lerp(48.0, 6.0, _Smoothness);
                half3 halfDir = normalize(mainLight.direction + viewDirWS);
                half spec = pow(saturate(dot(normalWS, halfDir)), specExp) * _Smoothness * _SpecularAnomaly;
                lighting += _SpecColor.rgb * mainLight.color.rgb * spec * 0.5;

                // Fluorescentes point: son additional lights.
                #if defined(_ADDITIONAL_LIGHTS)
                // LIGHT_LOOP_BEGIN, en su variante Forward+ (cluster), lee campos de
                // una variable llamada 'inputData'. El proyecto hoy va en Forward
                // (m_RenderingMode: 0) y no la usaria, pero si alguien cambia a
                // Forward+ el shader dejaria de compilar sin esto.
                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = viewDirWS;
                inputData.normalizedScreenSpaceUV = screenUV;

                uint addCount = GetAdditionalLightsCount();
                LIGHT_LOOP_BEGIN(addCount)
                    Light addLight = GetAdditionalLight(lightIndex, input.positionWS, half4(1, 1, 1, 1));
                    half aNdotL = saturate(dot(normalWS, addLight.direction));
                    half3 addAtten = addLight.color.rgb * addLight.distanceAttenuation * addLight.shadowAttenuation;
                    lighting += albedo * addAtten * aNdotL * 0.6;

                    half3 aHalf = normalize(addLight.direction + viewDirWS);
                    half aSpec = pow(saturate(dot(normalWS, aHalf)), specExp) * _Smoothness * _SpecularAnomaly;
                    lighting += _SpecColor.rgb * addAtten * aSpec * 0.5;
                LIGHT_LOOP_END
                #endif

                #if defined(_EMISSION)
                    lighting += _EmissionColor.rgb;
                #endif

                // Niebla por capitulo: vive en RenderSettings (lighting_profiles.yaml),
                // no en propiedades del material. El shader anterior declaraba
                // _FogDensity/_FogColor y no las usaba nunca, asi que la identidad
                // de niebla de cada capitulo no llegaba a estos 22 materiales.
                lighting = MixFog(lighting, input.fogFactor);

                return half4(lighting, _BaseColor.a);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            #pragma target 4.5
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct SAttr { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct SVar  { float4 positionCS : SV_POSITION; };

            SVar ShadowVert(SAttr input)
            {
                SVar output;

                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
                #else
                    float3 lightDirectionWS = _LightDirection;
                #endif

                // Aqui NO se aplica PS1SnapClipPos: este pase rasteriza en el clip
                // space de la LUZ, con _ScreenParams siendo el tamano del shadowmap,
                // asi que la rejilla no seria la misma que ve la camara. El desfase
                // resultante es menor que el shadow bias.
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #endif

                output.positionCS = positionCS;
                return output;
            }

            half4 ShadowFrag(SVar input) : SV_Target { return 0; }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma vertex DepthVert
            #pragma fragment DepthFrag
            #pragma target 4.5

            struct DAttr { float4 positionOS : POSITION; };
            struct DVar  { float4 positionCS : SV_POSITION; };

            DVar DepthVert(DAttr input)
            {
                DVar output;
                float4 positionCS = TransformObjectToHClip(input.positionOS.xyz);
                // Mismo snap que el forward: este pase si comparte el clip space
                // de la camara, asi que la profundidad coincide con lo que se ve.
                output.positionCS = PS1SnapClipPos(positionCS, _SnapPixelSize);
                return output;
            }

            half4 DepthFrag(DVar input) : SV_Target { return 0; }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }

            ZWrite On

            HLSLPROGRAM
            #pragma vertex DepthNormalsVert
            #pragma fragment DepthNormalsFrag
            #pragma target 4.5

            struct DNAttr
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct DNVar
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS   : TEXCOORD0;
            };

            DNVar DepthNormalsVert(DNAttr input)
            {
                DNVar output;
                float4 positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.positionCS = PS1SnapClipPos(positionCS, _SnapPixelSize);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            // Sin este pase el SSAO en modo "Depth Normals" no tiene normales que
            // leer para estos materiales. El shader anterior solo tenia DepthOnly.
            half4 DepthNormalsFrag(DNVar input) : SV_Target
            {
                return half4(normalize(input.normalWS), 0.0);
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}
