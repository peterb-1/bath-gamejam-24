Shader "Custom/Background"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _MainTexStrength ("Main Texture Strength", Range(0, 1)) = 0.5
        _Color ("Tint", Color) = (1,1,1,1)

        _SwirlStrength ("Swirl Strength", Range(0, 2)) = 1
        _SwirlScale ("Swirl Scale", Float) = 4
        _SwirlSpeed ("Swirl Speed", Float) = 0.1

        _GradientTopColor ("Top Color", Color) = (0.05, 0.05, 0.1, 1)
        _GradientBottomColor ("Bottom Color", Color) = (0.0, 0.0, 0.0, 1)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" }
        LOD 100
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _MainTexStrength;

            float _StarIntensity;
            float _StarSpeed;
            float _NoiseScale;
            float _TwinkleAmount;

            float _SwirlStrength;
            float _SwirlScale;
            float _SwirlSpeed;

            fixed4 _Color;
            fixed4 _GradientTopColor;
            fixed4 _GradientBottomColor;

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

            // Fade function for smooth interpolation (Perlin-style)
            float fade(float t) {
                return t * t * t * (t * (t * 6 - 15) + 10);
            }

            // Hash function returning float in [0,1]
            float hash21(float2 p) {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            // 2D smooth noise with smooth interpolation and jittered UVs
            float smoothNoise(float2 uv, float jitterAmount) {
                float2 jitter = float2(0.37, 0.91) * jitterAmount;
                uv += jitter;

                float2 i = floor(uv);
                float2 f = frac(uv);

                float u = fade(f.x);
                float v = fade(f.y);

                float a = hash21(i);
                float b = hash21(i + float2(1.0, 0.0));
                float c = hash21(i + float2(0.0, 1.0));
                float d = hash21(i + float2(1.0, 1.0));

                float lerpX1 = lerp(a, b, u);
                float lerpX2 = lerp(c, d, u);

                return lerp(lerpX1, lerpX2, v);
            }

            // Swirl noise as sum of octaves of smooth noise
            float swirlNoise(float2 uv, float time, float scale, float speed) {
                float2 pos = uv * scale + time * speed;
                float value = 0;
                float freq = 1;
                float amp = 1;

                for (int i = 0; i < 4; i++) {
                    value += smoothNoise(pos * freq, 0.1) * amp;
                    freq *= 2.0;
                    amp *= 0.5;
                }

                return value;
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);

                float a = hash21(i);
                float b = hash21(i + float2(1.0, 0.0));
                float c = hash21(i + float2(0.0, 1.0));
                float d = hash21(i + float2(1.0, 1.0));

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float swirlNoise(float2 uv)
            {
                float2 pos = uv * _SwirlScale + _Time.y * _SwirlSpeed;
                float value = 0;
                float freq = 1;
                float amp = 1;

                for (int i = 0; i < 4; i++)
                {
                    value += noise(pos * freq) * amp;
                    freq *= 2.0;
                    amp *= 0.5;
                }

                return value;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float time = _Time.y;
                float swirl = swirlNoise(i.uv, time, _SwirlScale, _SwirlSpeed) * _SwirlStrength;

                float4 gradient = lerp(_GradientBottomColor, _GradientTopColor, i.uv.y);
                float4 texColor = tex2D(_MainTex, i.uv) * _MainTexStrength;
                float3 finalColor = gradient.rgb + swirl.xxx;

                finalColor = lerp(finalColor, texColor.rgb + finalColor, _MainTexStrength);

                fixed4 col = texColor;
                col.rgb *= finalColor;
                col *= _Color;

                if (col.a < 0.01) discard;

                return col;
            }
            ENDCG
        }
    }
}
