Shader "Ink/Brush"
{
    // A textured brush stroke for meshes and trails: slash arcs, parry rings, sparks.
    // The stroke paints in along U (_Reveal) and then dries away, weakest bristles first (_Dry).
    Properties
    {
        _MainTex("Stroke (alpha)", 2D) = "white" {}
        _Color("Colour", Color) = (0.07, 0.065, 0.06, 1)
        _Reveal("Reveal along stroke", Range(0, 1.2)) = 1.2
        _Dry("Dry out", Range(0, 1)) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 4
    }

    SubShader
    {
        Tags { "Queue" = "Transparent+10" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest [_ZTest]
        Cull Off

        Pass
        {
            Name "Brush"
            Tags { "LightMode" = "UniversalForward" }

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
                half4 _Color;
                half _Reveal;
                half _Dry;
                half _ZTest;
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
                half4 color : COLOR;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float a = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv).a;
                float g = SAMPLE_TEXTURE2D(_InkGrainTex, sampler_InkGrainTex, i.uv * float2(2.0, 1.0)).r;

                // Painting in: a ragged leading edge moving along the stroke.
                float head = _Reveal + (g - 0.5) * 0.10;
                a *= smoothstep(head, head - 0.06, i.uv.x);

                // Drying out: thin bristles vanish first.
                a *= smoothstep(_Dry, _Dry + 0.22, a * (0.55 + 0.45 * g) + (1.0 - _Dry) * 0.22);

                half4 c = _Color * i.color;
                c.a *= saturate(a);
                return c;
            }
            ENDHLSL
        }
    }
    FallBack Off
}
