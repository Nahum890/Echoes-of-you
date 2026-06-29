Shader "Echoes/EchoGlow"
{
    Properties
    {
        _Color ("Base Color", Color) = (0.18, 0.9, 1.0, 0.46)
        _GlowColor ("Glow Color", Color) = (0.02, 0.65, 1.0, 1.0)
        _GlowPower ("Glow Power", Range(0.5, 8.0)) = 2.0
        _GlowIntensity ("Glow Intensity", Range(0.1, 10.0)) = 1.7
        _FlickerSpeed ("Flicker Speed", Range(0.0, 50.0)) = 14.0
        _DistortionSpeed ("Distortion Speed", Range(0.0, 10.0)) = 1.5
        _DistortionAmount ("Distortion Amount", Range(0.0, 0.2)) = 0.05
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
            };

            float4 _Color;
            float4 _GlowColor;
            float _GlowPower;
            float _GlowIntensity;
            float _FlickerSpeed;
            float _DistortionSpeed;
            float _DistortionAmount;

            v2f vert (appdata v)
            {
                v2f o;
                
                // Add slight temporal vertex displacement (distortion) based on time and normal
                float sinTime = sin(_Time.y * _DistortionSpeed + v.vertex.y * 3.0);
                float3 displacedPos = v.vertex.xyz + v.normal * sinTime * _DistortionAmount;
                
                o.pos = UnityObjectToClipPos(float4(displacedPos, 1.0));
                
                // World normal and view direction calculations
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                
                float3 worldPos = mul(unity_ObjectToWorld, float4(displacedPos, 1.0)).xyz;
                o.viewDir = normalize(_WorldSpaceCameraPos - worldPos);
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 normal = normalize(i.worldNormal);
                float3 viewDir = normalize(i.viewDir);
                
                // Fresnel calculation: brighter at grazing angles
                float fresnel = 1.0 - saturate(dot(normal, viewDir));
                fresnel = pow(fresnel, _GlowPower);
                
                // Temporal pulse flicker
                float pulse = sin(_Time.y * _FlickerSpeed) * 0.1 + 0.9;
                
                // Combine base color and Fresnel edge glow
                fixed4 finalColor = _Color + _GlowColor * fresnel * _GlowIntensity * pulse;
                
                // Enforce transparency rules
                finalColor.a = saturate(_Color.a + fresnel * _GlowIntensity * 0.5) * pulse;
                
                return finalColor;
            }
            ENDCG
        }
    }
}
