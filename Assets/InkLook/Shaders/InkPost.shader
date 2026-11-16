Shader "Ink/InkPost"
{
    // Full-screen ink pass. Used by a URP Full Screen Pass Renderer Feature with Depth + Normal requirements.
    Properties
    {
        _InkColor("Line colour (a = opacity)", Color) = (0.07, 0.065, 0.06, 0.96)
        _LineWidth("Outline width (px at 1080p)", Range(0.5, 12)) = 4.2
        _LineWidthVar("Brush pressure variation", Range(0, 1)) = 0.85
        _LineBreak("Dry-brush breakup", Range(0, 1)) = 0.55
        _CreaseStrength("Inner crease strength", Range(0, 1)) = 0.5
        _DepthThreshold("Depth edge threshold", Range(0.001, 0.2)) = 0.012
        _NormalSens("Normal edge sensitivity", Range(0, 4)) = 1.2
        _LineFade("Line fade distance (start, range)", Vector) = (30, 70, 0, 0)
        _WashWidth("Edge wash width (px)", Range(0, 40)) = 16
        _WashStrength("Edge wash strength", Range(0, 1)) = 0.3
        _Wobble("Hand wobble (px)", Range(0, 8)) = 2.0
        _WobbleScale("Wobble scale", Float) = 0.010
        _PaperStrength("Paper grain", Range(0, 1)) = 0.55
        _Vignette("Vignette", Range(0, 1)) = 0.3
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off
        ZTest Always
        Cull Off
        Blend Off

        Pass
        {
            Name "InkPost"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

            float4 _InkColor;
            float _LineWidth;
            float _LineWidthVar;
            float _LineBreak;
            float _CreaseStrength;
            float _DepthThreshold;
            float _NormalSens;
            float4 _LineFade;
            float _WashWidth;
            float _WashStrength;
            float _Wobble;
            float _WobbleScale;
            float _PaperStrength;
            float _Vignette;

            // Globals driven by gameplay (InkWorld / GameFeel).
            float4 _InkPaperColor;
            float _InkBleed;     // 0..1, wet stains creeping in from the screen edges
            float _InkDissolve;  // 0..1, whole picture washes out
            float _InkFlash;     // 0..1, flash of bare paper
            float _InkInvert;    // 0..1, ink negative used for the parry beat
            TEXTURE2D(_InkGrainTex);
            SAMPLER(sampler_InkGrainTex);

            float ink_hash(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float ink_vnoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float a = ink_hash(i);
                float b = ink_hash(i + float2(1, 0));
                float c = ink_hash(i + float2(0, 1));
                float d = ink_hash(i + float2(1, 1));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float ink_fbm(float2 p)
            {
                float v = 0.0;
                float a = 0.5;
                for (int i = 0; i < 4; i++)
                {
                    v += a * ink_vnoise(p);
                    p = p * 2.03 + 17.1;
                    a *= 0.5;
                }
                return v;
            }

            // Raw depth, larger = nearer on every platform. Raw depth is affine in screen space
            // for a flat surface, so second differences are zero on planes however steep they are.
            float NearDepth(float2 uv)
            {
                float r = SampleSceneDepth(uv);
                #if !UNITY_REVERSED_Z
                    r = 1.0 - r;
                #endif
                return r;
            }

            static const float2 kDirs[4] = { float2(1.0, 0.0), float2(0.7071, 0.7071), float2(0.0, 1.0), float2(-0.7071, 0.7071) };
            static const float2 kDirs2[4] = { float2(0.9239, 0.3827), float2(0.3827, 0.9239), float2(-0.3827, 0.9239), float2(-0.9239, 0.3827) };

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;
                float2 res = _ScreenParams.xy;
                float k = res.y / 1080.0;
                float2 ppx = uv * res / k; // pixel coordinates at a 1080p reference, so the look is resolution independent
                float3 paperCol = _InkPaperColor.rgb;
                float4 grain = SAMPLE_TEXTURE2D(_InkGrainTex, sampler_InkGrainTex, ppx / 512.0);

                // 1. Hand wobble: nothing in the picture is perfectly straight.
                float2 wob = float2(ink_vnoise(ppx * _WobbleScale + 3.7), ink_vnoise(ppx * _WobbleScale + 51.3)) - 0.5;
                wob += (float2(ink_vnoise(ppx * _WobbleScale * 3.3 + 8.1), ink_vnoise(ppx * _WobbleScale * 3.3 + 27.9)) - 0.5) * 0.45;
                float2 wuv = uv + wob * (_Wobble * 2.0 * k) / res;

                float3 col = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, wuv).rgb;

                // 2. Brush outlines. Four opposite tap pairs at a radius set by "pressure": the brush presses
                //    and lifts along a stroke, so lines swell and thin instead of holding one width.
                float rc = NearDepth(wuv);
                float eye = LinearEyeDepth(SampleSceneDepth(wuv), _ZBufferParams);
                float nearness = 1.0 - saturate((eye - _LineFade.x) / max(_LineFade.y, 0.001));
                float p1 = ink_vnoise(ppx * 0.0045 + 11.0);
                float p2 = ink_vnoise(ppx * 0.017 + 3.3);
                float pressure = saturate(p1 * p1 * 1.5 + p2 * 0.35);
                float radius = _LineWidth * 0.5 * k * lerp(1.0, 0.25 + 1.75 * pressure, _LineWidthVar) * lerp(0.35, 1.0, nearness);

                float dd = 0.0;
                float nd = 0.0;
                float nearest = rc;
                for (int i = 0; i < 4; i++)
                {
                    float2 o = kDirs[i] * radius / res;
                    float rA = NearDepth(wuv + o);
                    float rB = NearDepth(wuv - o);
                    nearest = max(nearest, max(rA, rB));
                    dd = max(dd, abs(rA + rB - 2.0 * rc));
                    float2 o2 = o * 0.45;
                    nd = max(nd, length(SampleSceneNormals(wuv + o2) - SampleSceneNormals(wuv - o2)));
                }
                dd /= max(nearest, 1e-6);
                float depthEdge = smoothstep(_DepthThreshold, _DepthThreshold * 2.5, dd);
                float normalEdge = smoothstep(0.55, 1.15, nd * _NormalSens) * _CreaseStrength;

                // Dry brush: bristle streaks break the line where the pressure is low.
                float bristle = SAMPLE_TEXTURE2D(_InkGrainTex, sampler_InkGrainTex, ppx * float2(0.0016, 0.0064)).r;
                float dry = smoothstep(0.30, 0.62, bristle + pressure * 0.55);
                float edge = saturate(max(depthEdge, normalEdge));
                edge *= lerp(1.0, dry, _LineBreak);
                edge *= lerp(0.25, 1.0, nearness);

                // 3. Edge wash: a soft grey gradient hugging every contour, as if the line had bled into damp paper.
                float wash = 0.0;
                if (_WashStrength > 0.001)
                {
                    float wr = _WashWidth * k * (0.6 + 0.8 * p1) * lerp(0.4, 1.0, nearness);
                    for (int j = 0; j < 4; j++)
                    {
                        float2 o = kDirs2[j] * wr / res;
                        float rA = NearDepth(wuv + o);
                        float rB = NearDepth(wuv - o);
                        wash += smoothstep(_DepthThreshold, _DepthThreshold * 4.0, abs(rA + rB - 2.0 * rc) / max(max(rc, max(rA, rB)), 1e-6));
                    }
                    wash = wash * 0.25 * (0.55 + 0.9 * grain.g);
                }
                col *= 1.0 - wash * _WashStrength * lerp(0.3, 1.0, nearness);
                float3 lineCol = lerp(_InkColor.rgb * 2.2 + 0.06, _InkColor.rgb, saturate(pressure * 1.6));
                col = lerp(col, lineCol, edge * _InkColor.a);

                // 4. Bleed: wet stains grow in from the edges of the page and soften what they cover.
                if (_InkBleed > 0.001)
                {
                    float aspect = res.x / res.y;
                    float ed = min(min(uv.x, 1.0 - uv.x) * aspect, min(uv.y, 1.0 - uv.y));
                    float blot = ink_fbm(ppx * 0.0035 + float2(0.0, _Time.y * 0.025));
                    float field = ed + (blot - 0.5) * 0.28;
                    float reach = 0.04 + _InkBleed * 0.30;
                    float m = 1.0 - smoothstep(reach * 0.55, reach, field);
                    float ring = smoothstep(reach * 0.88, reach * 0.98, field) * (1.0 - smoothstep(reach * 0.98, reach * 1.04, field));
                    float3 acc = 0.0;
                    for (int s = 0; s < 6; s++)
                    {
                        float a = 1.0472 * s + blot * 6.2832;
                        float2 off = float2(cos(a), sin(a)) * (10.0 * k * m) / res;
                        acc += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, wuv + off).rgb;
                    }
                    float3 wet = lerp(acc / 6.0, paperCol, 0.35);
                    col = lerp(col, wet, m * saturate(_InkBleed * 1.5));
                    col *= 1.0 - ring * 0.2 * saturate(_InkBleed * 2.0); // tide line where the stain dries
                }

                // 5. Parry beat: for a few frames the page turns into its own negative.
                if (_InkInvert > 0.001)
                {
                    float lumI = dot(col, float3(0.299, 0.587, 0.114));
                    float3 chroma = col - lumI;
                    float3 neg = lerp(paperCol, _InkColor.rgb, saturate(lumI * 1.25)) + chroma * 1.3;
                    col = lerp(col, neg, saturate(_InkInvert));
                }

                // 6. Wash-out: the whole picture dissolves into the paper.
                if (_InkDissolve > 0.001)
                {
                    float dn = ink_fbm(ppx * 0.005 + 20.0);
                    float th = _InkDissolve * 1.25 - 0.12;
                    float d = smoothstep(dn - 0.08, dn + 0.08, th);
                    float dr = smoothstep(dn - 0.16, dn - 0.06, th) * (1.0 - d);
                    col = lerp(col * (1.0 - dr * 0.4), paperCol, d);
                }

                col = lerp(col, paperCol, saturate(_InkFlash) * 0.7);

                // 7. Paper: fibres and grain on top of everything.
                float fibre = grain.b;
                float speck = ink_hash(floor(ppx));
                col *= 1.0 - _PaperStrength * (0.09 * (1.0 - fibre) + 0.03 * speck);
                float lum = dot(col, float3(0.299, 0.587, 0.114));
                col += (fibre - 0.5) * 0.05 * _PaperStrength * (1.0 - lum);

                float2 c = uv - 0.5;
                col *= 1.0 - _Vignette * 0.45 * smoothstep(0.12, 0.62, dot(c, c));

                return half4(max(col, 0.0), 1.0);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
