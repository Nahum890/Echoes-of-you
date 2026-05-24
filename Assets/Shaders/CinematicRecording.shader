Shader "Hidden/Echoes/CinematicRecording"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Intensity, _Scanlines, _Grain, _Flicker, _Desaturate, _EffectTime;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float lum = dot(col.rgb, float3(0.299, 0.587, 0.114));
                col.rgb = lerp(col.rgb, lum.xxx, _Desaturate * _Intensity);

                float scan = sin((i.uv.y + _EffectTime * 0.35) * 900.0) * 0.5 + 0.5;
                col.rgb *= 1.0 - scan * _Scanlines * _Intensity;

                float n = hash(i.uv * 1200.0 + _EffectTime * 40.0);
                col.rgb += (n - 0.5) * _Grain * _Intensity;

                float flick = 1.0 + sin(_EffectTime * 37.0) * _Flicker * _Intensity;
                col.rgb *= flick;

                float ab = _Intensity * 0.004;
                col.r = tex2D(_MainTex, i.uv + float2(ab, 0)).r;
                col.b = tex2D(_MainTex, i.uv - float2(ab, 0)).b;
                return col;
            }
            ENDCG
        }
    }
}
