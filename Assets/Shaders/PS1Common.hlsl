#ifndef ECHOES_PS1_COMMON_INCLUDED
#define ECHOES_PS1_COMMON_INCLUDED

// Utilidades compartidas por Echoes/PS1World y Echoes/PS1Character.
// Estan aqui y no duplicadas en cada shader porque el snapping tiene que dar
// EXACTAMENTE el mismo resultado en el pase forward y en los pases de
// profundidad; si se desincronizan, el SSAO y el depth fog dibujan bordes
// que no coinciden con la geometria visible.

// Snapping de vertices en clip space.
//
// Este es el efecto real de PS1: la GTE trabajaba con enteros y no tenia
// precision subpixel, asi que los vertices caian sobre la rejilla del
// framebuffer y "temblaban" al mover la camara. Hacerlo en espacio de OBJETO
// (como estaba antes) solo deforma el modelo una vez y no tiembla nunca.
//
// snapPixelSize = cada cuantos pixeles internos cae la rejilla. 0 lo desactiva.
float4 PS1SnapClipPos(float4 clipPos, float snapPixelSize)
{
    if (snapPixelSize <= 0.0)
    {
        return clipPos;
    }

    // Detras de la camara o justo en el plano: dejar pasar sin tocar, si no
    // la division revienta y aparecen triangulos disparados por la pantalla.
    if (abs(clipPos.w) < 1e-5)
    {
        return clipPos;
    }

    // NDC va de -1 a 1, de ahi el factor 2.
    float2 grid = _ScreenParams.xy / (2.0 * snapPixelSize);
    float2 ndc = clipPos.xy / clipPos.w;
    ndc = floor(ndc * grid + 0.5) / grid;
    clipPos.xy = ndc * clipPos.w;

    return clipPos;
}

// Ruido de valor barato para las manchas de humedad.
// Solo se evalua si _StainStrength > 0; ver la guarda en el fragment.
float PS1Hash(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

float PS1Noise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    return lerp(lerp(PS1Hash(i), PS1Hash(i + float2(1, 0)), f.x),
                lerp(PS1Hash(i + float2(0, 1)), PS1Hash(i + float2(1, 1)), f.x), f.y);
}

// 3 octavas, no 5. Las dos ultimas aportaban detalle que la cuantizacion de
// color del pase full-screen se come igualmente, a cambio de 8 sin() por pixel.
float PS1Fbm(float2 p)
{
    float v = 0.0;
    float a = 0.5;

    [unroll]
    for (int i = 0; i < 3; i++)
    {
        v += a * PS1Noise(p);
        p *= 2.0;
        a *= 0.5;
    }

    return v;
}

#endif // ECHOES_PS1_COMMON_INCLUDED
