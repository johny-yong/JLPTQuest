Shader "Ink/Stroke"
{
    // Soft-edged ink stroke for rain, wind streaks, splashes and trails. Texture-free.
    Properties
    {
        _Color("Colour", Color) = (0.37, 0.35, 0.32, 0.55)
        _Soft("Edge softness", Range(0.01, 1)) = 0.7
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "Stroke"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Soft;
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
                float ax = smoothstep(0.0, _Soft, 1.0 - abs(i.uv.x * 2.0 - 1.0));
                float ay = smoothstep(0.0, _Soft, 1.0 - abs(i.uv.y * 2.0 - 1.0));
                half4 c = _Color * i.color;
                c.a *= ax * ay;
                return c;
            }
            ENDHLSL
        }
    }
    FallBack Off
}
