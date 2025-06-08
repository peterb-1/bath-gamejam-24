Shader "Custom/GhostPlayer"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        // Ghost effect properties
        _GhostAlpha ("Ghost Alpha", Range(0.1, 0.8)) = 0.4
        _EdgeGlow ("Edge Glow", Range(0, 2)) = 0.5
        _DesatAmount ("Desaturation", Range(0,1)) = 0.7
        _LightenAmount ("Lighten Amount", Range(0,1)) = 0.3
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"

            // Additional properties
            fixed _GhostAlpha;
            fixed _WispIntensity;
            fixed _WispSpeed;
            fixed _WispScale;
            fixed _EdgeGlow;
            fixed _DesatAmount;
            fixed _LightenAmount;
            
            // Texture properties
            float4 _MainTex_TexelSize;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif
                
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // Get base sprite color with tint
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
                
                // Early exit if completely transparent
                if (c.a <= 0.01)
                    discard;

                float2 uv = IN.texcoord;
                
                // Edge detection for glow effect (simplified)
                float2 texelSize = _MainTex_TexelSize.xy;
                
                // Simple edge detection (sample neighboring pixels)
                float center = c.a;
                float left = tex2D(_MainTex, uv + float2(-texelSize.x, 0)).a;
                float right = tex2D(_MainTex, uv + float2(texelSize.x, 0)).a;
                float up = tex2D(_MainTex, uv + float2(0, texelSize.y)).a;
                float down = tex2D(_MainTex, uv + float2(0, -texelSize.y)).a;
                
                float edge = saturate(abs(center - left) + abs(center - right) + 
                               abs(center - up) + abs(center - down));
                
                // Add edge glow
                float glowContribution = edge * _EdgeGlow;
                c.rgb += glowContribution * c.rgb;
                
                // Apply ghost transparency while preserving original sprite's alpha mask
                c.a = c.a * _GhostAlpha + glowContribution;
                
                // Desaturate color
                float gray = dot(c.rgb, float3(0.299, 0.587, 0.114));
                fixed3 desatColor = lerp(c.rgb, float3(gray, gray, gray), _DesatAmount);

                // Lighten towards white
                desatColor = lerp(desatColor, float3(1.0, 1.0, 1.0), _LightenAmount);

                c.rgb = desatColor;
                
                return c;
            }
            ENDCG
        }
    }
}