Shader "Custom/BarrierFog"
{
    Properties
    {
        _FogColor ("Fog Color", Color) = (0.08, 0.1, 0.16, 1)
        _BarrierGlowColor ("Barrier Glow Color", Color) = (0.4, 1.0, 0.8, 1)

        _MapHalfWidth ("Map Half Width (world units)", Float) = 10
        _MapHalfHeight ("Map Half Height (world units)", Float) = 10
        _EdgeSoftness ("Edge Softness", Float) = 0.08

        _FogOpacity ("Fog Opacity", Range(0,1)) = 1

        _NoiseScale1 ("Noise Scale 1", Float) = 3
        _NoiseScale2 ("Noise Scale 2", Float) = 5
        _NoiseSpeed1 ("Noise Drift 1 (xy)", Vector) = (0.05, 0.03, 0, 0)
        _NoiseSpeed2 ("Noise Drift 2 (xy)", Vector) = (-0.02, 0.06, 0, 0)

        _MaxBlurSize ("Max Blur Size (screen UV)", Float) = 0.01
        _PulseSpeed ("Seam Pulse Speed", Float) = 1.6
        _SeamWidth ("Seam Band Width", Float) = 0.05
    }

    SubShader
    {
        // Transparent queue so it renders AFTER the opaque pass -- this is what lets
        // us read back the already-rendered terrain via the camera opaque texture.
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        ZWrite Off
        Cull Off
        // Output alpha is always 1 below (we composite terrain ourselves), so this
        // just writes our color straight over the framebuffer -- no extra blending.
        Blend One Zero

        Pass
        {
            Name "BarrierFogUnlit"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                float4 screenPos   : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _FogColor;
                float4 _BarrierGlowColor;
                float  _MapHalfWidth;
                float  _MapHalfHeight;
                float  _EdgeSoftness;
                float  _FogOpacity;
                float  _NoiseScale1;
                float  _NoiseScale2;
                float4 _NoiseSpeed1;
                float4 _NoiseSpeed2;
                float  _MaxBlurSize;
                float  _PulseSpeed;
                float  _SeamWidth;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs vpi = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionHCS = vpi.positionCS;
                OUT.positionWS  = vpi.positionWS;
                OUT.screenPos   = ComputeScreenPos(vpi.positionCS);
                return OUT;
            }

            // ---- cheap value noise (stand-in for the Gradient Noise node) ----
            float hash(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float valueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float a = hash(i);
                float b = hash(i + float2(1, 0));
                float c = hash(i + float2(0, 1));
                float d = hash(i + float2(1, 1));
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float t = _Time.y;

                // 1. Rectangular "distance from map" -- 0 at center, 1 at border, >1 outside
                float2 worldXY = IN.positionWS.xy;
                float distX = abs(worldXY.x) / _MapHalfWidth;
                float distY = abs(worldXY.y) / _MapHalfHeight;
                float dist = max(distX, distY);

                // 2. Soft mask that turns on just past the border
                float edgeMask = saturate((dist - 1.0) / _EdgeSoftness);

                // 3. Two-octave drifting noise for the swirl
                float2 uvA = worldXY * (_NoiseScale1 * 0.1) + _NoiseSpeed1.xy * t;
                float2 uvB = worldXY * (_NoiseScale2 * 0.1) + _NoiseSpeed2.xy * t;
                float noise = valueNoise(uvA) * 0.5 + valueNoise(uvB) * 0.5;

                // 4. Combine mask + noise, then make it fully opaque a couple of
                //    map-widths out so it never looks patchy far away
                float swirl = edgeMask * noise;
                float farFalloff = saturate((dist - 1.0) / 2.0);
                float density = max(swirl, farFalloff) * _FogOpacity;

                // 5. Blur amount grows with distance past the border
                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                float blurSize = lerp(0.0, _MaxBlurSize, saturate((dist - 1.0) / 0.4));

                float3 sceneSum  = SampleSceneColor(screenUV);
                sceneSum += SampleSceneColor(screenUV + float2( blurSize, 0));
                sceneSum += SampleSceneColor(screenUV + float2(-blurSize, 0));
                sceneSum += SampleSceneColor(screenUV + float2(0,  blurSize));
                sceneSum += SampleSceneColor(screenUV + float2(0, -blurSize));
                float3 blurredScene = sceneSum / 5.0;

                // 6. Seam glow: thin pulsing band right at the barrier edge
                float3 fogColor = _FogColor.rgb;
                float pulse = 0.6 + 0.4 * sin(t * _PulseSpeed);
                float seamBand = 1.0 - saturate(abs(dist - 1.0) / _SeamWidth);
                fogColor = lerp(fogColor, _BarrierGlowColor.rgb, seamBand * pulse);

                // 7. Final composite: sharp/blurred terrain fading into fog color
                float3 finalColor = lerp(blurredScene, fogColor, density);

                return float4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}
