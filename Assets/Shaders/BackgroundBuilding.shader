﻿Shader "Custom/BackgroundBuilding"
{
    Properties
    {
        _MainTex      ("Sprite", 2D) = "white" {}
        _Color        ("Sprite Renderer Color", Color) = (1,1,1,1)
        _BlurAmount   ("Blur Pixels", Range(0, 4)) = 1.5
        _FogColor     ("Fog Color", Color) = (0.6,0.7,0.9,1)
        _FogStrength  ("Max Fog Strength", Range(0,1)) = 0.6
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
            float  _BlurAmount;
            fixed4 _FogColor;
            float  _FogStrength;

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
                float2 px = _MainTex_TexelSize.xy * _BlurAmount;
                fixed4 c = tex2D(_MainTex, i.uv);
                c += tex2D(_MainTex, i.uv +  px);
                c += tex2D(_MainTex, i.uv + -px);
                c += tex2D(_MainTex, i.uv + float2( px.x,-px.y));
                c += tex2D(_MainTex, i.uv + float2(-px.x, px.y));
                c /= 5;
                c *= i.color;
                c.rgb = lerp(c.rgb, _FogColor.rgb, _FogStrength);

                return c;
            }
            ENDCG
        }
    }
}