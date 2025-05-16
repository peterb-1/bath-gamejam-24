Shader "Custom/Background03"
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
        _StarLayer3Color ("Star Layer 3 Color", Color) = (0.6, 0.8, 1.0, 1)
        _StarLayerSpeed ("Star Layer Speed Difference", Range(0.1, 5)) = 1.5
        
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
        
        [Header(Deep Space)]
        _GradientTopColor ("Top Color", Color) = (0.05, 0.05, 0.1, 1)
        _GradientBottomColor ("Bottom Color", Color) = (0.0, 0.0, 0.0, 1)
        _GradientExponent ("Gradient Exponent", Range(0.1, 5)) = 1.2
        _DeepSpaceStars ("Deep Space Stars", Range(0, 1)) = 0.3
        
        [Header(Cosmic Dust)]
        _DustIntensity ("Dust Intensity", Range(0, 1)) = 0.2
        _DustScale ("Dust Scale", Range(10, 100)) = 50
        _DustSpeed ("Dust Motion Speed", Range(0, 1)) = 0.05
        _DustColor ("Dust Color", Color) = (0.4, 0.3, 0.6, 1)
        
        [Header(Interstellar Clouds)]
        _CloudsIntensity ("Clouds Intensity", Range(0, 1)) = 0.3
        _CloudsScale ("Clouds Scale", Range(1, 10)) = 3
        _CloudsSpeed ("Clouds Motion Speed", Range(0, 0.5)) = 0.02
        _CloudsColor ("Clouds Color", Color) = (0.2, 0.3, 0.7, 1)
        
        [Header(Wormhole Effect)]
        _WormholeActive ("Wormhole Active", Range(0, 1)) = 0
        _WormholeCenter ("Wormhole Center", Vector) = (0.5, 0.5, 0, 0)
        _WormholeRadius ("Wormhole Radius", Range(0, 1)) = 0.2
        _WormholeStrength ("Wormhole Strength", Range(0, 5)) = 1.5
        _WormholeSpeed ("Wormhole Speed", Range(0, 5)) = 1
        
        [Header(PostProcessing)]
        _Vignette ("Vignette Strength", Range(0, 1)) = 0.3
        _VignetteColor ("Vignette Color", Color) = (0, 0, 0, 1)
        _Contrast ("Contrast", Range(0, 2)) = 1.1
        _Saturation ("Saturation", Range(0, 2)) = 1.2
        _Brightness ("Brightness", Range(0, 2)) = 1.0
        _HDRMultiplier ("HDR Multiplier", Range(1, 5)) = 1.2
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
            float4 _StarLayer3Color;
            float _StarLayerSpeed;

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
            float _DeepSpaceStars;

            // Dust properties
            float _DustIntensity;
            float _DustScale;
            float _DustSpeed;
            float4 _DustColor;
            
            // Cloud properties
            float _CloudsIntensity;
            float _CloudsScale;
            float _CloudsSpeed;
            float4 _CloudsColor;
            
            // Wormhole properties
            float _WormholeActive;
            float4 _WormholeCenter;
            float _WormholeRadius;
            float _WormholeStrength;
            float _WormholeSpeed;
            
            // Post-processing
            float _Vignette;
            float4 _VignetteColor;
            float _Contrast;
            float _Saturation;
            float _Brightness;
            float _HDRMultiplier;

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

            float fade(float t) {
                return t * t * t * (t * (t * 6 - 15) + 10);
            }

            float hash21(float2 p) {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }
            
            float hash11(float p) {
                p = frac(p * 7.13);
                p = p * p + p;
                return frac(p * p);
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

                return lerp(lerp(a, b, u), lerp(c, d, u), v);
            }

            float fbm(float2 uv, float time, float distortion) {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;
                float2 shift = float2(100, 100);
                
                // Domain warping for more organic look
                float2 warp = float2(0, 0);
                
                for(int i = 0; i < 6; i++) {
                    // Apply self-distortion for more organic look
                    float2 p = uv * frequency + shift + time * (0.1 * float2(cos(time*0.1), sin(time*0.13)) * distortion) + warp;
                    
                    float noiseVal = smoothNoise(p, 0.1);
                    value += amplitude * noiseVal;
                    
                    // Create domain warping for next iteration
                    warp.x = sin(uv.y * 5.0 + time * 0.1 + value * distortion * 4.0);
                    warp.y = cos(uv.x * 5.0 + time * 0.1 + value * distortion * 4.0);
                    
                    shift *= 1.6;
                    amplitude *= 0.5;
                    frequency *= 2.0;
                }
                
                return value;
            }

            float swirlNoise(float2 uv, float time, float scale, float speed, float layers, float distortion) {
                float2 pos = uv * scale + time * speed;
                float value = 0;
                float freq = 1;
                float amp = 1;
                
                // Add self-distortion
                float2 distort = float2(
                    fbm(uv + float2(time * 0.05, 0.0), time, 0.2),
                    fbm(uv + float2(0.0, time * 0.06), time, 0.2)
                ) * distortion;
                
                pos += distort;

                for (int i = 0; i < layers; i++) {
                    value += smoothNoise(pos * freq, 0.2) * amp;
                    freq *= 2.0;
                    amp *= 0.5;
                }

                return value;
            }

            float2 swirlUV(float2 uv, float time, float amount, float distortion) {
                float2 center = float2(0.5, 0.5);
                float2 offset = uv - center;

                float dist = length(offset);
                float angle = atan2(offset.y, offset.x);
                
                // Add time-dependent distortion to the swirl
                float distortFactor = sin(dist * 5.0 + time * 0.2) * distortion;
                angle += amount * sin(dist * _SwirlScale * 6.2831 - time * _SwirlSpeed) + distortFactor;

                float2 rotated = float2(cos(angle), sin(angle)) * dist;
                return center + rotated;
            }

            float2 starDriftUV(float2 uv, float time, float layer)
            {
                // Make each layer move differently for parallax effect
                float speedMod = 1.0 + (layer - 1.0) * 0.3;
                
                // More complex motion pattern
                uv.x += sin(uv.y * 10.0 + time * speedMod) * _StarDriftAmount;
                uv.y += cos(uv.x * 10.0 + time * 1.2 * speedMod) * _StarDriftAmount;
                // Add subtle rotation
                float angle = time * 0.03 * speedMod;
                float2 center = float2(0.5, 0.5);
                float2 centered = uv - center;
                float2 rotated = float2(
                    centered.x * cos(angle) - centered.y * sin(angle),
                    centered.x * sin(angle) + centered.y * cos(angle)
                );
                uv = rotated + center;
                
                return uv;
            }

            float starShape(float2 uv, float radius, float glow) {
                float dist = length(uv);
                
                // Core of the star
                float core = smoothstep(radius, radius * 0.5, dist);
                
                // Glow around the star
                float halo = smoothstep(radius * 3.0, radius, dist) * glow * 0.5;
                
                // Rays emanating from the star (subtle)
                float rays = 0.0;
                if (dist < radius * 3.0) {
                    float angle = atan2(uv.y, uv.x);
                    float rays1 = 0.5 + 0.5 * sin(angle * 8.0);
                    float rays2 = 0.5 + 0.5 * sin(angle * 6.0 + 0.7);
                    rays = max(rays1, rays2) * smoothstep(radius * 3.0, radius, dist) * 0.3;
                }
                
                return core + halo + rays;
            }

            float3 starColor(float seed, float colorVar) {
                // Base white color for stars
                float3 baseColor = float3(1.0, 1.0, 1.0);
                
                // Add color variation based on seed
                float r = 1.0 - colorVar * 0.5 * hash11(seed + 0.1);
                float g = 1.0 - colorVar * 0.7 * hash11(seed + 0.2);
                float b = 1.0 - colorVar * 0.3 * hash11(seed + 0.3);
                
                // Guarantee we don't have too-dark stars
                float minBrightness = 0.7;
                r = max(r, minBrightness);
                g = max(g, minBrightness);
                b = max(b, minBrightness);
                
                // Create temperature variations from hot blue to cool red
                if (hash11(seed + 0.4) > 0.7) {
                    // Blueish
                    b *= 1.2;
                    r *= 0.8;
                } else if (hash11(seed + 0.5) > 0.6) {
                    // Reddish
                    r *= 1.2;
                    b *= 0.8;
                } else if (hash11(seed + 0.6) > 0.8) {
                    // Yellowish
                    r *= 1.1;
                    g *= 1.1;
                    b *= 0.7;
                }
                
                return baseColor * float3(r, g, b);
            }

            float starLayer(float2 uv, float noiseScale, float time, float twinkleAmount, float starIntensity, float density,
               float colorVar, float sizeVar, float minSize, float maxSize, float glow, float3 layerTint,
               float visibleTime, float cycleLength)
            {
                float3 brightness = float3(0, 0, 0);
                float2 gridUV = uv * noiseScale * density;
                float2 baseCell = floor(gridUV);

                for (int x = -1; x <= 1; x++) {
                    for (int y = -1; y <= 1; y++) {
                        float2 cell = baseCell + float2(x, y);
                        float n = hash21(cell);

                        // Skip some cells based on density
                        if (n > 0.97 - density * 0.3) {
                            // Each star has a unique lifetime offset
                            float phaseOffset = frac(n * 11.13);
                            
                            // Use the cycleLength parameter to determine full cycle duration
                            float starTime = frac((time / cycleLength) + phaseOffset);

                            // Use visibleTime parameter to control how long the star is visible in each cycle
                            // Normalized to be between 0 and 1 relative to the cycle
                            float normVisibleTime = visibleTime / cycleLength;
                            
                            // Calculate visibility envelope based on cycle position
                            // Star is only visible for visibleTime portion of the cycle
                            float cyclePos = frac(starTime);
                            float visibilityWindow = smoothstep(0, normVisibleTime * 0.5, cyclePos) * 
                                                     smoothstep(0, normVisibleTime * 0.5, normVisibleTime - cyclePos);
                            
                            // Add twinkle effect within the visibility window
                            float pulseWindow = normVisibleTime * 0.5;
                            float twinkleFactor = 1.0 - smoothstep(0.0, pulseWindow, abs(cyclePos - (normVisibleTime * 0.5)));
                            
                            // Combine visibility and twinkle
                            float envelope = visibilityWindow * (1.0 - twinkleAmount + twinkleAmount * twinkleFactor);
                            
                            // Randomize star position and size
                            float2 starPos = cell + float2(n, frac(n * 13.1));
                            float2 diff = gridUV - starPos;

                            // Apply size variation
                            float sizeRand = frac(n * 9.3);
                            float size = lerp(minSize, maxSize, sizeRand * sizeVar + (1 - sizeVar) * 0.5);
                            
                            // Get star shape and color
                            float star = starShape(diff, size, glow);
                            float3 starCol = starColor(n, colorVar);
                            
                            // Apply layer tint
                            starCol *= layerTint;
                            
                            brightness += star * envelope * starIntensity * starCol;
                        }
                    }
                }

                return brightness;
            }

            float nebulaLayer(float2 uv, float time, float layers, float speed, float distortion)
            {
                float2 center = float2(0.5, 0.5);
                float2 centered = uv - center;
                
                // Rotate over time
                float angle = time * 0.05;
                float2 rot = float2(
                    centered.x * cos(angle) - centered.y * sin(angle),
                    centered.x * sin(angle) + centered.y * cos(angle)
                );
                
                uv = rot + center;
                
                // Domain warping for more organic nebula shapes
                float2 warp1 = float2(
                    smoothNoise(uv * 3.0 + time * 0.1, 0.3),
                    smoothNoise(uv * 3.0 + float2(3.2, 1.3) + time * 0.1, 0.3)
                ) * distortion;
                
                uv += warp1;
                
                return fbm(uv, time * speed, distortion * 0.5);
            }
            
            float dustLayer(float2 uv, float time, float scale, float speed) {
                float2 p = uv * scale;
                p.y += time * speed * 0.2;
                p.x += sin(p.y * 0.2 + time * 0.1) * 0.3;
                
                return smoothNoise(p, 0.5) * smoothNoise(p * 0.3, 0.5);
            }
            
            float cloudLayer(float2 uv, float time, float scale, float speed) {
                // Create base cellular pattern
                float2 p = uv * scale;
                
                // Add slow movement
                p.x += time * speed;
                p.y += sin(time * speed * 0.5) * 0.2;
                
                // Domain warping for more cloud-like appearance
                float warp = fbm(p * 0.5, time * 0.1, 0.2) * 2.0;
                p += warp;
                
                // Create cloud shapes
                float result = fbm(p, time * 0.05, 0.5);
                result = pow(result, 1.5); // Sharpen edges
                
                return result;
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
            
            float3 applyVignette(float3 color, float2 uv, float strength, float3 vignetteColor) {
                float2 center = float2(0.5, 0.5);
                float dist = length(uv - center) * 1.414; // Normalize to max distance
                float vignette = 1.0 - pow(smoothstep(0.0, 0.8, dist), 2.0) * strength;
                return lerp(vignetteColor, color, vignette);
            }
            
            float3 adjustContrast(float3 color, float contrast) {
                float midpoint = 0.5;
                return (color - midpoint) * contrast + midpoint;
            }
            
            float3 adjustSaturation(float3 color, float saturation) {
                float luminance = dot(color, float3(0.299, 0.587, 0.114));
                return lerp(luminance.xxx, color, saturation);
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

                // Swirled UV for background
                float2 bgUV = swirlUV(uv, time, _UVSwirlAmount, _SwirlDistortion);

                // Deep space background gradient with slight noise
                float gradientT = pow(uv.y, _GradientExponent);
                float4 gradient = lerp(_GradientBottomColor, _GradientTopColor, gradientT);
                
                // Add subtle deep space stars to gradient
                if (_DeepSpaceStars > 0.01) {
                    float deepStars = smoothNoise(uv * 500.0, 0.5);
                    deepStars = pow(deepStars, 15.0) * 5.0 * _DeepSpaceStars;
                    gradient.rgb += deepStars.xxx;
                }

                // Main swirl effect with enhanced detail
                float swirl = swirlNoise(bgUV, time, _SwirlScale, _SwirlSpeed, _SwirlLayers, _SwirlDistortion) * _SwirlStrength;
                
                // Nebula effect with two colors
                float nebula1 = nebulaLayer(bgUV, time, _NebulaLayers, _NebulaSpeed, _NebulaDistortion) * _NebulaIntensity;
                float nebula2 = nebulaLayer(bgUV * 1.3 + float2(0.7, 0.3), time * 0.7, _NebulaLayers, _NebulaSpeed * 0.8, _NebulaDistortion) * _NebulaIntensity;
                
                // Apply contrast to nebula
                nebula1 = pow(nebula1, _NebulaContrast);
                nebula2 = pow(nebula2, _NebulaContrast);
                
                // Mask nebulas to avoid filling the entire background
                float mask = smoothNoise(bgUV * 2.0, 0.2);
                mask = smoothstep(0.4, 0.6, mask);
                nebula1 *= mask;
                nebula2 *= (1.0 - mask);
                
                // Dust effect
                float dust = dustLayer(bgUV, time, _DustScale, _DustSpeed) * _DustIntensity;
                
                // Interstellar clouds
                float clouds = cloudLayer(bgUV, time, _CloudsScale, _CloudsSpeed) * _CloudsIntensity;
                
                // Star layers with different movement speeds and colors for parallax effect
                float2 starUV1 = starDriftUV(uv, time, 1);
                float2 starUV2 = starDriftUV(uv, time * 0.7, 2);
                float2 starUV3 = starDriftUV(uv, time * 0.4, 3);
                
                float3 stars1 = starLayer(starUV1, _NoiseScale, time * _StarSpeed, _TwinkleAmount, 
                                         _StarIntensity, _StarDensity, _StarColorVariation, 
                                         _StarSizeVariation, _StarMinSize, _StarMaxSize, _StarGlow, 
                                         _StarLayer1Color.rgb, _StarVisibleTime, _StarCycleLength);
                                         
                float3 stars2 = starLayer(starUV2, _NoiseScale * 0.7, time * _StarSpeed * 0.6, _TwinkleAmount * 1.2, 
                                         _StarIntensity * 0.8, _StarDensity * 0.8, _StarColorVariation, 
                                         _StarSizeVariation, _StarMinSize * 1.2, _StarMaxSize * 1.2, _StarGlow * 0.8, 
                                         _StarLayer2Color.rgb, _StarVisibleTime * 1.2, _StarCycleLength * 1.3);
                                         
                float3 stars3 = starLayer(starUV3, _NoiseScale * 0.5, time * _StarSpeed * 0.4, _TwinkleAmount * 1.5, 
                                         _StarIntensity * 0.6, _StarDensity * 0.6, _StarColorVariation, 
                                         _StarSizeVariation, _StarMinSize * 1.5, _StarMaxSize * 1.5, _StarGlow * 0.6, 
                                         _StarLayer3Color.rgb, _StarVisibleTime * 1.5, _StarCycleLength * 1.6);

                // Texture blend
                float4 texColor = tex2D(_MainTex, bgUV) * _MainTexStrength;

                // Composite all layers
                float3 finalColor = gradient.rgb;
                finalColor += swirl.xxx * 0.5;
                finalColor += nebula1 * _NebulaColor1.rgb;
                finalColor += nebula2 * _NebulaColor2.rgb;
                finalColor += dust * _DustColor.rgb;
                finalColor += clouds * _CloudsColor.rgb;
                finalColor += stars1 + stars2 + stars3;
                
                // Apply texture blend
                finalColor = lerp(finalColor, texColor.rgb + finalColor, _MainTexStrength);
                
                // Apply post-processing effects
                finalColor = applyVignette(finalColor, uv, _Vignette, _VignetteColor.rgb);
                finalColor = adjustContrast(finalColor, _Contrast);
                finalColor = adjustSaturation(finalColor, _Saturation);
                finalColor *= _Brightness;
                
                // Apply HDR multiplier for bright spots (stars, etc.)
                float luminance = dot(finalColor, float3(0.299, 0.587, 0.114));
                float highlightMask = smoothstep(0.7, 0.9, luminance);
                finalColor *= 1.0 + highlightMask * (_HDRMultiplier - 1.0);
                
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