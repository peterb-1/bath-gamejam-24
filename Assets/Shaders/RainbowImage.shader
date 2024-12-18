Shader "UI/GradientEffect"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _Speed ("Speed", Float) = 1.0
        _Strength ("Strength", Float) = 1.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "CanvasShader"="True" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            // Properties
            sampler2D _MainTex;
            float _Speed;
            float _Strength;

            // Constants
            static const float PI = 3.1415926538;
            static const float2 r_centre = float2(1.3, 0.6);
            static const float2 g_centre = float2(0.6, -0.3);
            static const float2 b_centre = float2(-0.3, 0.9);

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                float4 color : COLOR; // Vertex color from the UI system
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR; // Pass color to the fragment shader
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord; // Pass UVs directly
                o.color = v.color; // Pass vertex color to the fragment
                return o;
            }

            float4 getGradient(float2 uv, float time)
            {
                float r_angle = atan2(uv.y - r_centre.y, uv.x - r_centre.x);
                float r_oscillator = sin(r_angle + time) * 0.5 + 0.5;

                float g_angle = atan2(uv.y - g_centre.y, uv.x - g_centre.x);
                float g_oscillator = sin(g_angle + time) * 0.5 + 0.5;

                float b_angle = atan2(uv.y - b_centre.y, uv.x - b_centre.x);
                float b_oscillator = sin(b_angle + time) * 0.5 + 0.5;

                return float4(r_oscillator, g_oscillator, b_oscillator, 1.0);
            }

            float4 frag (v2f i) : SV_Target
            {
                // Sample the main texture
                float4 texColor = tex2D(_MainTex, i.uv);

                // Calculate the gradient
                float2 uv = i.uv;
                float time = _Time.y * _Speed;
                float4 gradient = getGradient(uv, time);

                // Combine texture, gradient, and vertex color
                float4 outputColor = (texColor * i.color) * (1 - _Strength) + (texColor * gradient * i.color) * _Strength;

                // Preserve alpha blending with the original texture
                outputColor.a = texColor.a * i.color.a;

                return outputColor;
            }
            ENDCG
        }
    }

    FallBack "UI/Default"
}
