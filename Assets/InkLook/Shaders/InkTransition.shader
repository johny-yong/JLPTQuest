Shader "Ink/Transition"
{
    // Scene transition drawn on a full-screen UI image.
    // Cover: broad brush strokes sweep across in alternating directions until the page is black.
    // Reveal: the wet ink thins and opens into the new scene.
    Properties
    {
        _MainTex("Stroke (alpha)", 2D) = "white" {}
        _Ink("Ink", Color) = (0.07, 0.065, 0.06, 1)
        _Cover("Cover", Range(0, 1)) = 0
        _Reveal("Reveal", Range(0, 1)) = 0
        _Bands("Stroke count", Float) = 5
    }

    SubShader
    {
        Tags { "Queue" = "Overlay" "RenderType" = "Transparent" "IgnoreProjector" = "True" "PreviewType" = "Plane" "CanUseSpriteAtlas" = "False" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_InkGrainTex);
            SAMPLER(sampler_InkGrainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Ink;
                float _Cover;
                float _Reveal;
                float _Bands;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            float hash1(float n) { return frac(sin(n * 91.345) * 47453.5453); }

            half4 frag(Varyings i) : SV_Target
            {
                float2 uv = i.uv;
                float aspect = _ScreenParams.x / _ScreenParams.y;
                float4 grain = SAMPLE_TEXTURE2D(_InkGrainTex, sampler_InkGrainTex, uv * float2(aspect, 1.0) * 1.5);

                // Low-frequency grain: long bristle streaks and broad blots, so edges tear instead of speckling.
                float streak = SAMPLE_TEXTURE2D(_InkGrainTex, sampler_InkGrainTex, float2(uv.x * 0.12, uv.y * 5.0)).r;
                float blot = SAMPLE_TEXTURE2D(_InkGrainTex, sampler_InkGrainTex, uv * float2(aspect, 1.0) * 0.2).g;

                // --- overlapping strokes, one per band.
                // Cover: each stroke's head runs across the screen. Reveal: its tail follows and lifts the ink off.
                float y = uv.y * _Bands;
                float baseBand = floor(y);
                float alpha = 0.0;
                float rim = 0.0;
                for (int j = -1; j <= 1; j++)
                {
                    float b = baseBand + j;
                    if (b < 0.0 || b > _Bands - 1.0) continue;
                    float vv = (y - (b + 0.5)) / 1.7 + 0.5;          // each stroke is 1.7 bands tall, so neighbours overlap
                    if (vv < 0.0 || vv > 1.0) continue;
                    float order = hash1(b + 3.0);                     // strokes start in a shuffled order
                    float p = saturate(_Cover * 1.5 - order * 0.5);
                    float head = p * 1.35;                            // tapered tip leaves the screen at the end
                    float u = fmod(b, 2.0) < 0.5 ? uv.x : 1.0 - uv.x;
                    float su = u / max(head, 1e-3);
                    if (su > 1.0) continue;
                    float a = SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, float2(su * 0.9 + 0.08, vv), 0).a;

                    float q = saturate(_Reveal * 1.5 - hash1(b + 11.0) * 0.5);
                    float tail = lerp(-0.45, 1.5, q) + (streak - 0.5) * 0.4 + (blot - 0.5) * 0.25;
                    float keep = smoothstep(tail - 0.02, tail + 0.05, u);
                    a *= keep;
                    rim = max(rim, a * (1.0 - smoothstep(tail + 0.05, tail + 0.32, u)));
                    alpha = max(alpha, a);
                }
                alpha = saturate(alpha * 1.5);
                float full = smoothstep(0.90, 1.0, _Cover) * (1.0 - smoothstep(0.0, 0.04, _Reveal));
                alpha = max(alpha, full);

                float3 col = _Ink.rgb + (grain.b - 0.5) * 0.03;
                col = lerp(col, col * 3.0 + 0.14, saturate(rim) * 0.55);   // the thinning edge is watery
                return half4(col, alpha * _Ink.a);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
