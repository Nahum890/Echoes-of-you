Shader "Echoes/PS1Character"
{
    // Personajes (Aiden, ecos solidos). Igual que PS1World, el dither y la
    // cuantizacion de color los pone la Renderer Feature "Echoes PS1 Post".
    //
    // A diferencia del mundo, aqui NO hay affine mapping: sobre una malla
    // animada el skew de las UV se mueve con cada frame de animacion y marea.
    Properties
    {
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _BaseTex ("Albedo", 2D) = "white" {}

        [Header(PS1)]
        _SnapPixelSize ("Rejilla de snap (px internos)", Range(0, 8)) = 1

        [Header(Lighting)]
        _FlatAmbient ("Ambiente plano", Range(0,2)) = 1.0
        _AmbientFloor ("Suelo de ambiente", Range(0,1)) = 0.25
        _Smoothness ("Smoothness", Range(0,1)) = 0.1
        [HDR] _EmissionColor ("Emission", Color) = (0,0,0,0)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 200

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "PS1Common.hlsl"

        // Las texturas van FUERA del CBUFFER. Estaban dentro, que es ilegal:
        // rompe la compatibilidad con el SRP Batcher y en algunas plataformas
        // directamente no compila.
        CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float4 _EmissionColor;
            float4 _BaseTex_ST;
            float _SnapPixelSize;
            float _FlatAmbient;
            float _AmbientFloor;
            float _Smoothness;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 4.5

            #pragma shader_feature_local _EMISSION

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseTex);
            SAMPLER(sampler_BaseTex);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float  fogFactor  : TEXCOORD3;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);

                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float4 positionCS = TransformWorldToHClip(positionWS);
                positionCS = PS1SnapClipPos(positionCS, _SnapPixelSize);

                output.positionCS = positionCS;
                output.positionWS = positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseTex);
                output.fogFactor = ComputeFogFactor(positionCS.z);

                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half3 albedo = _BaseColor.rgb * SAMPLE_TEXTURE2D(_BaseTex, sampler_BaseTex, input.uv).rgb;

                half3 normalWS = normalize(input.normalWS);
                half3 viewDirWS = normalize(GetWorldSpaceViewDir(input.positionWS));
                float2 screenUV = GetNormalizedScreenSpaceUV(input.positionCS);

                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                Light mainLight = GetMainLight(shadowCoord);

                // El shader anterior solo tenia la contribucion de la direccional:
                // ni SH, ni ambiente, ni luces adicionales. Como los niveles se
                // iluminan con point lights fluorescentes (que son additional
                // lights), el personaje se quedaba practicamente negro.
                half3 shAmbient = SampleSH(normalWS);
                half3 ambient = max(shAmbient, half3(_AmbientFloor, _AmbientFloor, _AmbientFloor * 1.02)) * _FlatAmbient;

                half NdotL = saturate(dot(normalWS, mainLight.direction));
                half3 directional = mainLight.color.rgb * NdotL *
                                    mainLight.distanceAttenuation * mainLight.shadowAttenuation;

                #if defined(_SCREEN_SPACE_OCCLUSION)
                    AmbientOcclusionFactor aoFactor = GetScreenSpaceAmbientOcclusion(screenUV);
                    ambient *= aoFactor.indirectAmbientOcclusion;
                    directional *= aoFactor.directAmbientOcclusion;
                #endif

                half3 lighting = albedo * (ambient + directional);

                half specExp = lerp(48.0, 6.0, _Smoothness);
                half3 halfDir = normalize(mainLight.direction + viewDirWS);
                half spec = pow(saturate(dot(normalWS, halfDir)), specExp) * _Smoothness;
                lighting += mainLight.color.rgb * spec * 0.5;

                #if defined(_ADDITIONAL_LIGHTS)
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
                    lighting += albedo * addAtten * aNdotL;

                    half3 aHalf = normalize(addLight.direction + viewDirWS);
                    lighting += addAtten * pow(saturate(dot(normalWS, aHalf)), specExp) * _Smoothness * 0.5;
                LIGHT_LOOP_END
                #endif

                #if defined(_EMISSION)
                    lighting += _EmissionColor.rgb;
                #endif

                lighting = MixFog(lighting, input.fogFactor);

                return half4(lighting, _BaseColor.a);
            }
            ENDHLSL
        }

        // El shader anterior no tenia ninguno de estos tres pases: el personaje
        // no proyectaba sombra, no escribia profundidad y no aportaba normales
        // al SSAO. El FallBack no cubre eso de forma fiable en URP.
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

            half4 DepthNormalsFrag(DNVar input) : SV_Target
            {
                return half4(normalize(input.normalWS), 0.0);
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}
