Shader "Custom/Background02"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _MainTexStrength ("Main Texture Strength", Range(0, 1)) = 0.5
        _Color ("Tint", Color) = (1,1,1,1)

        _StarIntensity ("Star Intensity", Range(0, 5)) = 1
        _StarSpeed ("Star Twinkle Speed", Range(0.1, 10)) = 1
        _NoiseScale ("Star Noise Scale", Float) = 10
        _TwinkleAmount ("Twinkle Amount", Range(0, 1)) = 0.5

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

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float random(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }

            float2 random2(float2 p)
            {
                return frac(sin(float2(dot(p, float2(127.1, 311.7)),
                                       dot(p, float2(269.5, 183.3)))) * 43758.5453);
            }

            float starShape(float2 uv, float2 pos, float2 rand)
            {
                float2 offset = uv - pos;

                // Compute rotation angle and stretch factor
                float angle = rand.x * 6.2831; // 0 to 2*pi
                float stretch = lerp(1.0, 3.0, rand.y);

                // Rotate offset by angle
                float cosA = cos(angle);
                float sinA = sin(angle);
                float2 rotatedOffset = float2(
                    offset.x * cosA - offset.y * sinA,
                    offset.x * sinA + offset.y * cosA
                );

                // Apply stretching on X axis (after rotation)
                rotatedOffset.x /= stretch;

                // Compute distance squared for smoothstep
                float d = dot(rotatedOffset, rotatedOffset);

                // Size controls star radius (smaller means sharper star)
                float size = lerp(0.0008, 0.0025, rand.y);

                return smoothstep(size, 0.0, d);
            }

            float starLayer(float2 uv)
            {
                float brightness = 0.0;
                float2 gridUV = uv * _NoiseScale;
                float2 base = floor(gridUV);

                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        float2 cell = base + float2(x, y);
                        float2 rand = random2(cell);
                        float2 starPos = (cell + rand) / _NoiseScale;

                        // Twinkle animation
                        float twinkle = sin(_Time.y * _StarSpeed + rand.x * 6.2831) * 0.5 + 0.5;
                        float twinkleFade = lerp(1.0, twinkle, _TwinkleAmount);

                        // Apply custom star shape
                        float shape = starShape(uv, starPos, rand);

                        // Vary brightness per star
                        float brightnessFactor = 0.2 + rand.x * 0.8;

                        brightness += shape * brightnessFactor * twinkleFade;
                    }
                }

                return brightness * _StarIntensity;
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
                float starGlow = starLayer(i.uv) * _StarIntensity;

                float swirl = swirlNoise(i.uv) * _SwirlStrength;
                float4 swirlColor = float4(swirl, swirl * 0.8, swirl * 1.2, 1.0);

                float4 gradient = lerp(_GradientBottomColor, _GradientTopColor, i.uv.y);

                float3 sceneColor = gradient.rgb + swirlColor.rgb + starGlow;
                sceneColor = saturate(sceneColor);

                float4 mainTexCol = tex2D(_MainTex, i.uv) * _MainTexStrength;
                sceneColor = lerp(sceneColor, sceneColor + mainTexCol.rgb, _MainTexStrength);

                sceneColor *= _Color.rgb;

                return fixed4(sceneColor, _Color.a);
            }
            ENDCG
        }
    }
}
