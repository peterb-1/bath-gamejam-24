Shader "Custom/Fog"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _MainTexStrength ("Main Texture Strength", Range(0, 1)) = 0.5
        _Color ("Tint", Color) = (1,1,1,1)
        
        [Header(Stars)]
        _StarIntensity ("Star Intensity", Range(0, 20)) = 1
        _StarSpeed ("Star Twinkle Speed", Range(0.1, 10)) = 1
        _NoiseScale ("Star Noise Scale", Float) = 10
        _TwinkleAmount ("Twinkle Amount", Range(0, 1)) = 0.5
        _StarDriftAmount ("Star Drift Amount", Range(0, 0.1)) = 0.01
        _StarDensity ("Star Density", Range(0.1, 3)) = 1
        _StarColorVariation ("Star Color Variation", Range(0, 1)) = 0.5
        _StarSizeVariation ("Star Size Variation", Range(0, 1)) = 0.5
        _StarMinSize ("Star Min Size", Range(0.005, 0.1)) = 0.03
        _StarMaxSize ("Star Max Size", Range(0.05, 0.2)) = 0.07
        _StarGlow ("Star Glow", Range(0, 5)) = 1.5
        _StarVisibleTime ("Star Visible Time", Range(0.05, 0.5)) = 0.15
        _StarCycleLength ("Star Cycle Length", Range(2, 15)) = 8
        
        [Header(Star Field Layers)]
        _StarLayer1Color ("Star Layer 1 Color", Color) = (0.9, 0.9, 1.0, 1)
        _StarLayer2Color ("Star Layer 2 Color", Color) = (1.0, 0.8, 0.6, 1)
        
        [Header(Background Swirl)]
        _SwirlStrength ("Swirl Noise Strength", Range(0, 2)) = 1
        _SwirlScale ("Swirl Scale", Float) = 4
        _SwirlSpeed ("Swirl Speed", Float) = 0.1
        _UVSwirlAmount ("UV Swirl Amount", Range(0, 2)) = 0.5
        _SwirlLayers ("Swirl Detail Layers", Range(1, 5)) = 4
        _SwirlDistortion ("Swirl Self-Distortion", Range(0, 1)) = 0.3
        
        [Header(Nebula)]
        _NebulaColor1 ("Nebula Color 1", Color) = (0.3, 0.2, 0.5, 1)
        _NebulaColor2 ("Nebula Color 2", Color) = (0.5, 0.2, 0.3, 1)
        _NebulaIntensity ("Nebula Intensity", Range(0, 2)) = 0.4
        _NebulaContrast ("Nebula Contrast", Range(0.1, 5)) = 1.8
        _NebulaLayers ("Nebula Detail Layers", Range(1, 5)) = 4
        _NebulaSpeed ("Nebula Motion Speed", Range(0, 0.5)) = 0.05
        _NebulaDistortion ("Nebula Distortion", Range(0, 1)) = 0.3
        
        [Header(Background)]
        _GradientTopColor ("Top Color", Color) = (0.05, 0.05, 0.1, 1)
        _GradientBottomColor ("Bottom Color", Color) = (0.0, 0.0, 0.0, 1)
        _GradientExponent ("Gradient Exponent", Range(0.1, 5)) = 1.2
        
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

            // Stars properties
            float _StarIntensity;
            float _StarSpeed;
            float _NoiseScale;
            float _TwinkleAmount;
            float _StarDriftAmount;
            float _StarDensity;
            float _StarColorVariation;
            float _StarSizeVariation;
            float _StarMinSize;
            float _StarMaxSize;
            float _StarGlow;
            float _StarVisibleTime;
            float _StarCycleLength;
            
            float4 _StarLayer1Color;
            float4 _StarLayer2Color;

            // Swirl properties
            float _SwirlStrength;
            float _SwirlScale;
            float _SwirlSpeed;
            float _SwirlLayers;
            float _SwirlDistortion;

            // Nebula properties
            float4 _NebulaColor1;
            float4 _NebulaColor2;
            float _NebulaIntensity;
            float _NebulaLayers;
            float _NebulaSpeed;
            float _NebulaContrast;
            float _NebulaDistortion;

            // Gradient properties
            fixed4 _Color;
            fixed4 _GradientTopColor;
            fixed4 _GradientBottomColor;
            float _GradientExponent;
            
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

            // Faster hash function
            float hash21(float2 p) {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }
            
            float hash11(float p) {
                p = frac(p * 7.13);
                return frac(p * p + p);
            }

            float hash12(float2 p) {
                float3 p3 = frac(float3(p.xyx) * float3(0.1031, 0.103, 0.0973));
                p3 += dot(p3, p3.yzx + 19.19);
                return frac((p3.x + p3.y) * p3.z);
            }
            
            float noise(float2 p) {
                float2 i = floor(p);
                float2 f = frac(p);
                
                float a = hash12(i);
                float b = hash12(i + float2(1.0, 0.0));
                float c = hash12(i + float2(0.0, 1.0));
                float d = hash12(i + float2(1.0, 1.0));
                
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }

            float smoothNoise(float2 uv, float jitterAmount) {
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

            float fbm(float2 uv, float time, float distortion) {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;
                
                // Reduce iterations for better performance
                for(int i = 0; i < 4; i++) {
                    float2 p = uv * frequency + time * 0.1 * float2(cos(time*0.1), sin(time*0.13)) * distortion;
                    value += amplitude * smoothNoise(p, 0.1);
                    frequency *= 2.0;
                    amplitude *= 0.5;
                }
                
                return value;
            }

            float swirlNoise(float2 uv, float time, float scale, float speed, float layers, float distortion) {
                float2 pos = uv * scale;
                float value = 0;
                float freq = 1;
                float amp = 1;

                // Apply time-based movement
                pos += time * speed;
                
                // Reduce iterations for better performance
                int maxLayers = min(int(layers), 3);
                for (int i = 0; i < maxLayers; i++) {
                    value += smoothNoise(pos * freq, 0.2) * amp;
                    freq *= 2.0;
                    amp *= 0.5;
                }

                return value;
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

            float2 starDriftUV(float2 uv, float time, float layer)
            {
                // Make each layer move differently for parallax effect
                float speedMod = 1.0 + (layer - 1.0) * 0.3;
                
                // Simplified drift pattern
                uv.x += sin(uv.y * 10.0 + time * speedMod) * _StarDriftAmount;
                uv.y += cos(uv.x * 10.0 + time * 1.2 * speedMod) * _StarDriftAmount;
                
                return uv;
            }

            float starShape(float2 uv, float radius, float glow) {
                float dist = length(uv);
                
                // Core of the star
                float core = smoothstep(radius, radius * 0.5, dist);
                
                // Glow around the star
                float halo = smoothstep(radius * 3.0, radius, dist) * glow * 0.5;
                
                return core + halo;
            }

            float3 starColor(float seed, float colorVar) {
                // Base white color for stars
                float3 baseColor = float3(1.0, 1.0, 1.0);
                
                // Add color variation based on seed
                float r = 1.0 - colorVar * 0.5 * hash11(seed + 0.1);
                float g = 1.0 - colorVar * 0.7 * hash11(seed + 0.2);
                float b = 1.0 - colorVar * 0.3 * hash11(seed + 0.3);
                
                // Ensure minimum brightness
                r = max(r, 0.7);
                g = max(g, 0.7);
                b = max(b, 0.7);
                
                return baseColor * float3(r, g, b);
            }

            float3 starLayer(float2 uv, float noiseScale, float time, float twinkleAmount, float starIntensity, float density,
               float colorVar, float sizeVar, float minSize, float maxSize, float glow, float3 layerTint,
               float visibleTime, float cycleLength)
            {
                float3 brightness = float3(0, 0, 0);
                float2 gridUV = uv * noiseScale * density;
                float2 baseCell = floor(gridUV);

                // Check only neighboring cells for better performance
                for (int x = -1; x <= 1; x++) {
                    for (int y = -1; y <= 1; y++) {
                        float2 cell = baseCell + float2(x, y);
                        float n = hash21(cell);

                        // Skip some cells based on density for optimization
                        if (n > 0.97 - density * 0.3) {
                            // Each star has a unique lifetime offset
                            float phaseOffset = frac(n * 11.13);
                            float starTime = frac((time / cycleLength) + phaseOffset);
                            float normVisibleTime = visibleTime / cycleLength;
                            
                            // Calculate visibility
                            float cyclePos = frac(starTime);
                            float visibilityWindow = smoothstep(0, normVisibleTime * 0.5, cyclePos) * 
                                                    smoothstep(0, normVisibleTime * 0.5, normVisibleTime - cyclePos);
                            
                            // Add twinkle effect
                            float twinkleFactor = 1.0 - smoothstep(0.0, normVisibleTime * 0.5, 
                                                abs(cyclePos - (normVisibleTime * 0.5)));
                            float envelope = visibilityWindow * (1.0 - twinkleAmount + twinkleAmount * twinkleFactor);
                            
                            // Star position and properties
                            float2 starPos = cell + float2(n, frac(n * 13.1));
                            float2 diff = gridUV - starPos;
                            float sizeRand = frac(n * 9.3);
                            float size = lerp(minSize, maxSize, sizeRand * sizeVar + (1 - sizeVar) * 0.5);
                            
                            // Get star appearance
                            float star = starShape(diff, size, glow);
                            float3 starCol = starColor(n, colorVar) * layerTint;
                            
                            brightness += star * envelope * starIntensity * starCol;
                        }
                    }
                }

                return brightness;
            }

            float nebulaLayer(float2 uv, float time, float layers, float speed, float distortion)
            {
                // Simplify nebula calculation by using fbm directly
                return fbm(uv, time * speed, distortion * 0.5);
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

                // Background gradient
                float gradientT = pow(uv.y, _GradientExponent);
                float4 gradient = lerp(_GradientBottomColor, _GradientTopColor, gradientT);

                // Main swirl effect
                float swirl = swirlNoise(bgUV, time, _SwirlScale, _SwirlSpeed, _SwirlLayers, _SwirlDistortion) * _SwirlStrength;
                
                // Nebula effect with two colors - simplified
                float nebula1 = nebulaLayer(bgUV, time, _NebulaLayers, _NebulaSpeed, _NebulaDistortion) * _NebulaIntensity;
                float nebula2 = nebulaLayer(bgUV * 1.3 + float2(0.7, 0.3), time * 0.7, _NebulaLayers, 
                                           _NebulaSpeed * 0.8, _NebulaDistortion) * _NebulaIntensity;
                
                // Apply contrast to nebula
                nebula1 = pow(nebula1, _NebulaContrast);
                nebula2 = pow(nebula2, _NebulaContrast);
                
                // Simplified mask
                float mask = smoothNoise(bgUV * 2.0, 0.2);
                mask = step(0.5, mask);
                nebula1 *= mask;
                nebula2 *= (1.0 - mask);
                
                // Star layers with different movement speeds
                float2 starUV1 = starDriftUV(uv, time, 1);
                float2 starUV2 = starDriftUV(uv, time * 0.7, 2);
                
                float3 stars1 = starLayer(starUV1, _NoiseScale, time * _StarSpeed, _TwinkleAmount, 
                                         _StarIntensity, _StarDensity, _StarColorVariation, 
                                         _StarSizeVariation, _StarMinSize, _StarMaxSize, _StarGlow, 
                                         _StarLayer1Color.rgb, _StarVisibleTime, _StarCycleLength);
                                         
                float3 stars2 = starLayer(starUV2, _NoiseScale * 0.7, time * _StarSpeed * 0.6, _TwinkleAmount * 1.2, 
                                         _StarIntensity * 0.8, _StarDensity * 0.8, _StarColorVariation, 
                                         _StarSizeVariation, _StarMinSize * 1.2, _StarMaxSize * 1.2, _StarGlow * 0.8, 
                                         _StarLayer2Color.rgb, _StarVisibleTime * 1.2, _StarCycleLength * 1.3);

                // Texture blend
                float4 texColor = tex2D(_MainTex, bgUV) * _MainTexStrength;

                // Composite all layers - simplified
                float3 finalColor = gradient.rgb;
                finalColor += swirl.xxx * 0.5;
                finalColor += nebula1 * _NebulaColor1.rgb;
                finalColor += nebula2 * _NebulaColor2.rgb;
                finalColor += stars1 + stars2;
                
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