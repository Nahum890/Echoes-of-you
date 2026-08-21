Shader "Echoes/PS1Post"
{
    // Pase full-screen que emula el framebuffer de PlayStation 1:
    //   1. dither ordenado 4x4 (la matriz real del hardware PSX)
    //   2. cuantizacion a N bits por canal (PS1 = 5 bits => 15-bit color)
    //   3. scanlines en espacio de pantalla
    //
    // Se ejecuta a la resolucion interna (RenderScale), ANTES del upscale
    // nearest-neighbour, para que el dither quede en pixeles gordos.
    // El look de escaneo NO va en los materiales: aqui cubre todo por igual
    // (opacos, transparentes, particulas, el eco).
    Properties
    {
        [Header(Color)]
        _ColorDepth       ("Bits por canal", Range(1, 8)) = 5
        _DitherStrength   ("Fuerza del dither", Range(0, 1)) = 1
        _DitherScale      ("Escala del dither (px)", Range(1, 4)) = 1

        [Header(CRT)]
        _ScanlineStrength ("Fuerza de scanlines", Range(0, 1)) = 0.12
        _ScanlinePeriod   ("Periodo de scanline (px)", Range(2, 8)) = 2
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            Name "PS1 Framebuffer"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.5

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _ColorDepth;
            float _DitherStrength;
            float _DitherScale;
            float _ScanlineStrength;
            float _ScanlinePeriod;

            // Matriz de dither real de la PSX: offsets aplicados al color de 8 bits
            // antes de truncar a 5. Su media es -0.5, por eso abajo se normaliza
            // con +4.5 y no con +4 (asi la media queda en 0.5 y no oscurece).
            static const float PSX_DITHER[16] =
            {
                -4.0,  0.0, -3.0,  1.0,
                 2.0, -2.0,  3.0, -1.0,
                -3.0,  1.0, -4.0,  0.0,
                 3.0, -1.0,  2.0, -2.0
            };

            float PsxDither(float2 pixel)
            {
                float2 cell = floor(pixel / max(1.0, _DitherScale));
                int2 idx = int2(fmod(cell, 4.0));
                return (PSX_DITHER[idx.y * 4 + idx.x] + 4.5) / 8.0;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half4 source = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, input.texcoord);

                // positionCS es la coordenada de pixel del render target de destino,
                // es decir la resolucion interna. Sirve tanto para el dither como
                // para las scanlines sin depender de _ScreenParams.
                float2 pixel = input.positionCS.xy;

                // Un framebuffer de 15 bits no guarda HDR: se recorta igual que la consola.
                half3 color = saturate(source.rgb);

                // La cuantizacion tiene que ocurrir en espacio de display, no lineal;
                // si no, las bandas se amontonan en las sombras y se ve mal.
                color = LinearToSRGB(color);

                float levels = exp2(_ColorDepth) - 1.0;
                float threshold = lerp(0.5, PsxDither(pixel), _DitherStrength);
                color = floor(color * levels + threshold) / levels;

                color = SRGBToLinear(saturate(color));

                // Scanlines a resolucion interna: una linea oscura cada _ScanlinePeriod px.
                float band = frac(pixel.y / max(2.0, _ScanlinePeriod));
                color *= lerp(1.0, 1.0 - _ScanlineStrength, step(0.5, band));

                return half4(color, source.a);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
