Shader "Custom/TitleFog"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _MainTexStrength ("Main Texture Strength", Range(0, 1)) = 0.5
        _Color ("Tint", Color) = (1,1,1,1)
        
        [Header(Background Swirl)]
        _SwirlStrength ("Swirl Noise Strength", Range(0, 2)) = 1
        _SwirlScale ("Swirl Scale", Float) = 4
        _SwirlSpeed ("Swirl Speed", Float) = 0.1
        _UVSwirlAmount ("UV Swirl Amount", Range(0, 2)) = 0.5
        
        [Header(Wormhole Effect)]
        _WormholeActive ("Wormhole Active", Range(0, 1)) = 0
        _WormholeCenter ("Wormhole Center", Vector) = (0.5, 0.5, 0, 0)
        _WormholeRadius ("Wormhole Radius", Range(0, 1)) = 0.2
        _WormholeStrength ("Wormhole Strength", Range(0, 5)) = 1.5
        _WormholeSpeed ("Wormhole Speed", Range(0, 5)) = 1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
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
            float _UVSwirlAmount;
            float4 _Color;
            
            // Swirl properties
            float _SwirlStrength;
            float _SwirlScale;
            float _SwirlSpeed;

            // Wormhole properties
            float _WormholeActive;
            float4 _WormholeCenter;
            float _WormholeRadius;
            float _WormholeStrength;
            float _WormholeSpeed;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            float hash21(float2 p) {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float smoothNoise(float2 uv) {
                float2 i = floor(uv);
                float2 f = frac(uv);

                float u = f.x * f.x * (3.0 - 2.0 * f.x);
                float v = f.y * f.y * (3.0 - 2.0 * f.y);

                float a = hash21(i);
                float b = hash21(i + float2(1.0, 0.0));
                float c = hash21(i + float2(0.0, 1.0));
                float d = hash21(i + float2(1.0, 1.0));

                return lerp(lerp(a, b, u), lerp(c, d, u), v);
            }

            float swirlNoise(float2 uv, float time, float scale, float speed) {
                float2 pos = uv * scale;
                pos += time * speed;
                return smoothNoise(pos);
            }

            float2 swirlUV(float2 uv, float time, float amount) {
                float2 center = float2(0.5, 0.5);
                float2 offset = uv - center;

                float dist = length(offset);
                float angle = atan2(offset.y, offset.x);
                
                // Add swirl effect
                angle += amount * sin(dist * _SwirlScale * 6.2831 - time * _SwirlSpeed);

                float2 rotated = float2(cos(angle), sin(angle)) * dist;
                return center + rotated;
            }

            float2 wormholeEffect(float2 uv, float time, float2 center, float radius, float strength, float speed) {
                float2 delta = uv - center;
                float dist = length(delta);
                
                // Create spinning effect inside the wormhole
                float angle = atan2(delta.y, delta.x);
                angle += time * speed;
                
                // Effect intensity falls off outside the radius
                float intensity = smoothstep(radius * 1.5, radius * 0.5, dist);
                
                // Create stretching effect toward the center
                float stretchFactor = intensity * strength * (1.0 - pow(dist / radius, 0.5));
                
                // Calculate new UV
                float2 offset = float2(cos(angle), sin(angle)) * stretchFactor * dist;
                return uv + offset;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float time = _Time.y;
                float2 uv = i.uv;
                
                // Apply wormhole distortion if active
                if (_WormholeActive > 0.01) {
                    uv = lerp(uv, wormholeEffect(uv, time, _WormholeCenter.xy, _WormholeRadius, 
                                                _WormholeStrength, _WormholeSpeed), _WormholeActive);
                }

                // Swirled UV for background - simplified
                float2 bgUV = swirlUV(uv, time, _UVSwirlAmount);

                // Main swirl effect
                float swirl = swirlNoise(bgUV, time, _SwirlScale, _SwirlSpeed) * _SwirlStrength;
                
                // Texture blend
                float4 texColor = tex2D(_MainTex, bgUV) * _MainTexStrength;

                // Composite all layers - simplified
                float3 finalColor = swirl.xxx;
                
                // Apply texture blend
                finalColor = lerp(finalColor, texColor.rgb + finalColor, _MainTexStrength);
                
                // Create final output
                fixed4 col = fixed4(finalColor, texColor.a);
                col *= _Color * i.color;

                if (col.a < 0.01) discard;
                return col;
            }
            ENDCG
        }
    }
}