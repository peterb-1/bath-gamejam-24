Shader "Custom/BackgroundBuilding"
{
    Properties
    {
        _MainTex      ("Sprite", 2D) = "white" {}
        _Color        ("Sprite Renderer Color", Color) = (1,1,1,1)
        _Tint         ("Tint", Color) = (1,1,1,1)

        _BlurAmount   ("Blur Pixels", Range(0, 4)) = 1.5
        _ShimmerAmp   ("Shimmer Amplitude", Range(0, 2)) = 0.4
        _ShimmerFreq  ("Shimmer Frequency", Float) = 6
        _ShimmerSpeed ("Shimmer Speed",  Float) = 0.2

        _FogColor     ("Fog Color", Color) = (0.6,0.7,0.9,1)
        _FogStrength  ("Max Fog Strength", Range(0,1)) = 0.6
        _LayerDepth   ("Layer Depth 0-1", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { "Queue"="Transparent"
               "RenderType"="Transparent"
               "IgnoreProjector"="True"
               "PreviewType"="Sprite" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4    _MainTex_TexelSize;
            fixed4    _Color;
            fixed4    _Tint;

            float  _BlurAmount;
            float  _ShimmerAmp;
            float  _ShimmerFreq;
            float  _ShimmerSpeed;

            fixed4 _FogColor;
            float  _FogStrength;
            float  _LayerDepth;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR; 
            };

            struct v2f
            {
                float4 pos    : SV_POSITION;
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR;
                float3 worldPos : TEXCOORD1;
            };

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34,456.21));
                p += dot(p, p+45.32);
                return frac(p.x * p.y);
            }

            float noise(float2 p)
            {
                float2 i = floor(p); float2 f=frac(p); float2 u=f*f*(3-2*f);
                return lerp( lerp(hash21(i),hash21(i+float2(1,0)),u.x),
                             lerp(hash21(i+float2(0,1)),hash21(i+float2(1,1)),u.x), u.y);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv;
                o.color = v.color * _Color;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                //------------------------------------------------
                // Screen-space shimmer offset based on world position
                //------------------------------------------------
                float time = _Time.y * _ShimmerSpeed;

                // Scale and offset world position for noise input
                float2 worldNoiseUV = i.worldPos.xy * _ShimmerFreq;
                float n = noise(worldNoiseUV + time);

                // Calculate offset for shimmer in screen UV space
                float2 off = (n - 0.5) * _ShimmerAmp * _MainTex_TexelSize.xy * 60; 

                //------------------------------------------------
                // 5-tap self-blur
                //------------------------------------------------
                float2 px = _MainTex_TexelSize.xy * _BlurAmount;
                fixed4 c = tex2D(_MainTex, i.uv + off);
                c += tex2D(_MainTex, i.uv + off +  px);
                c += tex2D(_MainTex, i.uv + off + -px);
                c += tex2D(_MainTex, i.uv + off + float2( px.x,-px.y));
                c += tex2D(_MainTex, i.uv + off + float2(-px.x, px.y));
                c /= 5;

                //------------------------------------------------
                // Apply colors: Vertex color, sprite color, and tint
                //------------------------------------------------
                c *= i.color;
                c *= _Tint;

                //------------------------------------------------
                // Fog
                //------------------------------------------------
                float fog = _FogStrength * _LayerDepth;
                c.rgb = lerp(c.rgb, _FogColor.rgb, fog);

                return c;
            }
            ENDCG
        }
    }
}